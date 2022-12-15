using System;
using System.Text;
using System.Linq;
using System.Data.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Collections.Generic;
using System.Collections.Concurrent;

namespace Data.Core
{
    public class ObjectRow: BaseRow
    {
        #region Public Classes
        public class PropertyDefinition
        {
            public bool IsEntityType => EntityFrameworkTypes.Contains(Type) || IsNullableEntityType(Type);
            public string Name { get; set; }
            public Type Type { get; set; }

            public PropertyDefinition(PropertyDefinition definition): this(definition.Name, definition.Type)
            { }

            public PropertyDefinition(PropertyInfo info): this(info.Name, info.PropertyType)
            { }            

            public PropertyDefinition(string name, Type type = null)
            {
                Name  = name;
                Type  = type;
            }

            #region Scary Stuff Written When I Was Tired
            //List compiled from http://msdn.microsoft.com/en-us/library/cc716729(v=vs.110).aspx
            private static readonly Type[] EntityFrameworkTypes = { typeof(string), typeof(char), typeof(int), typeof(long), typeof(byte[]), typeof(bool), typeof(DateTime), typeof(DateTimeOffset), typeof(decimal), typeof(double), typeof(float), typeof(short), typeof(Guid) };

            private static bool IsNullableEntityType(Type type)
            {
                return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>) && EntityFrameworkTypes.Contains(type.GenericTypeArguments[0]);
            }
            #endregion
        }

        public class PropertyValue
        {
            public string Name { get; set; }
            public object Value { get; set; }

            public PropertyValue(PropertyInfo info, object @object) : this(info.Name, info.GetValue(@object))
            { }

            public PropertyValue(string name, object value)
            {
                Name  = name;
                Value = value;
            }
            public void SetFor(object instance)
            {
                instance.GetType().GetProperty(Name, BindingFlags.Public | BindingFlags.Instance).SetValue(instance, Value);
            }
        }
        #endregion

        #region Private Classes
        private static class MyTypeBuilder
        {
            private static readonly ConcurrentDictionary<string, Type> Types = new ConcurrentDictionary<string, Type>();

            public static object CreateNewObject(IEnumerable<PropertyDefinition> definitions)
            {
                definitions = definitions.ToArray();
                
                var definitionString = String(definitions);

                if (!Types.ContainsKey(definitionString))
                {
                    Types.TryAdd(definitionString, Type(definitions));
                }

                return Activator.CreateInstance(Types[definitionString]);
            }

            private static TypeBuilder GetTypeBuilder()
            {
                var assemblyName = new AssemblyName("MyDynamicAssembly");
                var assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
                var moduleBuilder = assemblyBuilder.DefineDynamicModule("MainModule");

                return moduleBuilder.DefineType(Guid.NewGuid().ToString()
                                              , TypeAttributes.Public
                                              | TypeAttributes.Class
                                              | TypeAttributes.AutoClass
                                              | TypeAttributes.AnsiClass
                                              | TypeAttributes.BeforeFieldInit
                                              | TypeAttributes.AutoLayout
                                              , null);
            }

            private static Type Type(IEnumerable<PropertyDefinition> properties)
            {
                var tb = GetTypeBuilder();
                
                AddConstructorToTypeBuilder(tb);

                foreach (var property in properties)
                {                    
                    AddPropertyToTypeBuilder(tb, property.Type, property.Name);
                }

                return tb.CreateType();
            }

            private static void AddConstructorToTypeBuilder(TypeBuilder tb)
            {
                tb.DefineDefaultConstructor(MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName);
            }

            private static void AddPropertyToTypeBuilder(TypeBuilder tb, Type propertyType, string propertyName)
            {
                const MethodAttributes methodAttributes = (MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig);

                var fieldBuilder    = tb.DefineField("_" + propertyName, propertyType, FieldAttributes.Private);
                var propertyBuilder = tb.DefineProperty(propertyName, PropertyAttributes.HasDefault, propertyType, null);
                var propertyGetBldr = tb.DefineMethod("get_" + propertyName, methodAttributes, propertyType, System.Type.EmptyTypes);
                var propertySetBldr = tb.DefineMethod("set_" + propertyName, methodAttributes, null, new[] { propertyType });
                var getIlGenerator  = propertyGetBldr.GetILGenerator();
                var setIlGenerator  = propertySetBldr.GetILGenerator();

                getIlGenerator.Emit(OpCodes.Ldarg_0);
                getIlGenerator.Emit(OpCodes.Ldfld, fieldBuilder);
                getIlGenerator.Emit(OpCodes.Ret);

                setIlGenerator.MarkLabel(setIlGenerator.DefineLabel());
                setIlGenerator.Emit(OpCodes.Ldarg_0);
                setIlGenerator.Emit(OpCodes.Ldarg_1);
                setIlGenerator.Emit(OpCodes.Stfld, fieldBuilder);

                setIlGenerator.Emit(OpCodes.Nop);
                setIlGenerator.MarkLabel(setIlGenerator.DefineLabel());
                setIlGenerator.Emit(OpCodes.Ret);

                propertyBuilder.SetGetMethod(propertyGetBldr);
                propertyBuilder.SetSetMethod(propertySetBldr);
            }

            private static string String(IEnumerable<PropertyDefinition> properties)
            {
                var sb = new StringBuilder();

                foreach (var property in properties)
                {
                    sb.Append(property.Name).Append(property.Type.FullName);//This has to be FullName or GUID
                }

                return string.Intern(sb.ToString());
            }
        }
        #endregion

        #region Fields
        private object _object;
        private static readonly ConcurrentDictionary<object, PropertyDefinition[]> DefinitionsCache = new ConcurrentDictionary<object, PropertyDefinition[]>();
        #endregion

        #region Properties
        public IEnumerable<object> RawValues => Columns.Select(RawValue);

        #endregion

        #region Constructors
        public ObjectRow()
        {
            _object = null;
        }

        public ObjectRow(object @object)
        {
            var objectRow = @object as ObjectRow;
            var otherRow  = @object as IRow;

            if (objectRow != null)
            {
                _object = objectRow._object;
            }
            else if (otherRow != null)
            {
                _object = new ObjectRow(otherRow.Select(kv => new PropertyDefinition(kv.Key, typeof(string))), otherRow.Select(kv => new PropertyValue(kv.Key, kv.Value)))._object;
            }
            else
            {
                _object = @object;
            }
        }

        public ObjectRow(IEnumerable<PropertyDefinition> definitions, IEnumerable<PropertyValue> values)
        {
           _object = MyTypeBuilder.CreateNewObject(definitions);

            foreach (var value in values)
            {
                value.SetFor(_object);
            }
        }
        #endregion

        #region Public Methods
        public override IEnumerable<string> Values
        {
            get { return Columns.Select(c => this[c]).ToArray(); }
        }

        public override IEnumerable<string> Columns => _object == null ? Enumerable.Empty<string>() : ObjectDefs().Select(p => p.Name);

        public override string this[string column]
        {
            get
            {
                return RawValue(column)?.ToString();
            }
            set
            {
                var property = _object?.GetType().GetProperty(column, BindingFlags.Instance | BindingFlags.Public);

                if(property == null) throw new ColumnNotFoundException(column);

                if (property.SetMethod == null)
                {
                    AddSetterAndSetValue(column, value.To(property.PropertyType));
                }
                else
                {
                    property.SetValue(_object, value.To(property.PropertyType));
                }
            }
        }

        public override void Add(string column, object value)
        {
            var type = value?.GetType() ?? typeof(string);
            var defs = ObjectDefs().ToList();
            var vals = ObjectVals().ToList(); //big time sink 1

            if (defs.Any(d => d.Name == column))
            {
                throw new DuplicateKeyException($"The {column} column already exists on the row");
            }

            defs.Add(new PropertyDefinition(column, type));
            vals.Add(new PropertyValue(column, value));

            _object = MyTypeBuilder.CreateNewObject(defs);

            vals.ForEach(v => v.SetFor(_object)); //big time sink 2
        }

        public override void Remove(string column)
        {
            var defs = ObjectDefs().Where(d => d.Name != column).ToArray();
            var vals = ObjectVals().Where(v => v.Name != column).ToArray();

            _object = MyTypeBuilder.CreateNewObject(defs);

            foreach (var value in vals)
            {
                value.SetFor(_object);
            }
        }
        
        public object RawValue(string column)
        {
            var property = _object?.GetType().GetProperty(column, BindingFlags.Instance | BindingFlags.Public);

            if (property == null) throw new ColumnNotFoundException(column);            

            return property.GetValue(_object);
        }

        public void RawValue(string column, object value)
        {
            var property = _object?.GetType().GetProperty(column, BindingFlags.Instance | BindingFlags.Public);

            if (property == null) throw new ColumnNotFoundException(column);

            property.SetValue(_object, value);
        }
        #endregion

        #region Private Methods
        private IEnumerable<PropertyDefinition> ObjectDefs()
        {
            if (_object == null) return Enumerable.Empty<PropertyDefinition>();

            var objectType = _object.GetType();

            if (!DefinitionsCache.ContainsKey(objectType))
            {
                DefinitionsCache.TryAdd(objectType, _object.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance).Select(p => new PropertyDefinition(p)).Where(p => p.IsEntityType).ToArray());
            }

            return DefinitionsCache[objectType];
        }

        //I've tried several different implementations. This is the fastest I can find of all the trivial options
        private IEnumerable<PropertyValue> ObjectVals()
        {
            return _object?.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance).Select(p => new PropertyValue(p, _object)).ToArray() ?? Enumerable.Empty<PropertyValue>();
        }

        private void AddSetterAndSetValue(string column, object value)
        {
            var definitions = ObjectDefs().ToArray();
            var values      = ObjectVals().ToArray();
            
            values.Single(d => d.Name == column).Value = value;

            _object = MyTypeBuilder.CreateNewObject(definitions);

            foreach (var v in values)
            {
                v.SetFor(_object);
            }
        }
        #endregion        
    }
}