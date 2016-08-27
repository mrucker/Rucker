using System;
using System.Linq;
using System.Threading;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using Rucker.Email;
using Rucker.Exceptions;
using Rucker.Extensions;

namespace Rucker.Entities
{
    public class DbEntityValidationExceptionEmailer: IExceptionEmailer
    {
        #region Fields
        private readonly string _application;
        private readonly string _environment;
        private readonly string _from;
        private readonly string _to;
        private readonly IEmailer _emailer;
        private readonly List<Exception> _emailedExceptions;
        #endregion

        #region Constructors
        public DbEntityValidationExceptionEmailer(string application, string environment, string from, string to, IEmailer emailer)
        {
            _application       = application;
            _environment       = environment;
            _from              = from;
            _to                = to;
            _emailer           = emailer;
            _emailedExceptions = new List<Exception>();
        }

        #endregion

        #region Public Methods
        public void Email(Exception ex, string extra = null)
        {
            if (ex == null || _emailedExceptions.Contains(ex)) return;

            _emailedExceptions.Add(ex);

            _emailer.Send(_from, _to, Subject(), Body(ex, extra));
        }
        #endregion

        #region Private Methods
        private string Subject()
        {
            return $"{_application} Error *{_environment}*";
        }
        private string Body(Exception ex, string extra)
        {
            var body = new System.Text.StringBuilder();

            if (Thread.CurrentPrincipal.Identity.IsAuthenticated)
            {
                body.AppendLine("[Username]");
                body.AppendLine(Thread.CurrentPrincipal.Identity.Name);
            }

            body.AppendLine();
            body.AppendLine("[Server]");
            body.AppendLine(Environment.MachineName);

            body.AppendLine();
            body.AppendLine("[Type]");
            body.AppendLine(Types(ex));

            body.AppendLine();
            body.AppendLine("[Message]");
            body.AppendLine(Messages(ex));

            if (ex is DbEntityValidationException)
            {
                body.AppendLine();
                body.AppendLine("[EntityValidationErrors]");
                body.AppendLine(ValidationMessages(ex as DbEntityValidationException));
            }

            if (extra != null)
            {
                body.AppendLine();
                body.AppendLine(extra);
            }

            body.AppendLine();
            body.AppendLine("[StackTrace]");
            body.AppendLine(StackTraces(ex));

            return body.ToString();
        }

        private static string Messages(Exception ex, int nestCount = 0)
        {
            if (ex == null) return "";

            var prepend = (nestCount == 0) ? "" : Environment.NewLine;
            var nest    = new string('-', nestCount);

            return prepend + nest + ex.Message + Messages(ex.InnerException, nestCount + 1);
        }

        private static string Types(Exception ex, int nestCount = 0)
        {
            if (ex == null) return "";

            var prepend = (nestCount == 0) ? "" : Environment.NewLine;
            var nest    = new string('-', nestCount);

            return prepend + nest + ex.GetType() + Types(ex.InnerException, nestCount + 1);
        }

        private static string StackTraces(Exception ex, int nestCount = 0)
        {
            if (ex == null) return "";

            var prepend = (nestCount == 0) ? "" : Environment.NewLine;
            var nest    = (nestCount == 0) ? "" : Environment.NewLine + "==============================================================================" + Environment.NewLine;
            
            return prepend + nest + ex.StackTrace + StackTraces(ex.InnerException, nestCount + 1);
        }

        private static string ValidationMessages(DbEntityValidationException ex)
        {
            return ex.EntityValidationErrors.SelectMany(v => v.ValidationErrors).Select(v => v.ErrorMessage).Cat(Environment.NewLine);
        }
        #endregion
    }
}