using System.Linq;
using System.Collections.Generic;
using Rucker.Extensions;

namespace Rucker.Security
{
    public class PasswordPolicy
    {
        #region Properties
        public int MinimumLength { get; }
        public int MaximumLength { get; }
        public bool RequireSymbol { get; }
        public bool RequireNumber { get; }
        public bool RequireLower { get; }
        public bool RequireUpper { get; }
        #endregion

        #region Constructors
        public PasswordPolicy(bool requireLower, bool requireUpper, bool requireNumber, bool requireSymbol, int minimumLength, int maximumLength)
        {
            MinimumLength = minimumLength;
            MaximumLength = maximumLength;
            RequireSymbol = requireSymbol;
            RequireNumber = requireNumber;
            RequireUpper  = requireUpper;
            RequireLower  = requireLower;
        }
        #endregion

        #region Public Methods
        public bool PasswordMeetsPolicy(string password)
        {
            password = password ?? "";

            var meetsLengthPolicy     = password.Length.Between(MinimumLength, MaximumLength);
            var meetsSymbolsPolicy    = !RequireSymbol || password.Any(c => char.IsSymbol(c) || char.IsPunctuation(c) || char.IsSeparator(c));
            var meetsNumbersPolicy    = !RequireNumber || password.Any(char.IsDigit);
            var meetsUpperCasesPolicy = !RequireUpper  || password.Any(char.IsUpper);
            var meetsLowerCasesPolicy = !RequireLower  || password.Any(char.IsLower);
            
            return meetsSymbolsPolicy && meetsNumbersPolicy && meetsUpperCasesPolicy && meetsLowerCasesPolicy && meetsLengthPolicy;
        }

        public IEnumerable<string> Failures(string password)
        {
            var failsLengthPolicy = password.Length < MinimumLength || MaximumLength < password.Length;
            var failsSymbolPolicy = RequireSymbol && password.None(c => char.IsSymbol(c) || char.IsPunctuation(c) || char.IsSeparator(c));
            var failsNumberPolicy = RequireNumber && password.None(char.IsDigit);
            var failsUpperPolicy  = RequireUpper && password.None(char.IsUpper);
            var failsLowerPolicy  = RequireLower && password.None(char.IsLower);

            if (failsLengthPolicy)
            {
                yield return $"Your password must be between {MinimumLength} to {MaximumLength} characters long";
            }

            if (failsSymbolPolicy)
            {
                yield return "Your password must contain at least one symbol";
            }

            if (failsNumberPolicy)
            {
                yield return "Your password must contain at least one number";
            }

            if (failsUpperPolicy)
            {
                yield return "Your password must contain at least one upper case letter";
            }

            if (failsLowerPolicy)
            {
                yield return "Your password must contain at least one upper case letter";
            }
        }

        public override string ToString()
        {
            var eachRequirementAsText = EachRequirementAsText().ToArray();
            var requirementsAsText    = eachRequirementAsText.Length == 1 ? eachRequirementAsText.Single() : $"{eachRequirementAsText.Take(eachRequirementAsText.Length-1).Cat(", ")} and {eachRequirementAsText.Last()}";

            return $"Your new password must { requirementsAsText }";
        }

        public string ToHtml()
        {
            return $"<p>Sorry, the new password must:</p> <ul style='list-style-type:disc; padding-left:30px;'>{EachRequirementAsText().Select(r => $"<li>{r}</li>").Cat()}</ul>";
        }
        #endregion

        #region Private Methods
        private IEnumerable<string> EachRequirementAsText()
        {

            if (MinimumLength > 0 && MaximumLength == int.MaxValue)
            {
                yield return $"be longer than {MinimumLength} characters";
            }
            else if (MinimumLength <= 0 && MaximumLength < int.MaxValue)
            {
                yield return $"be shorter than {MaximumLength} characters";
            }
            else if (MinimumLength > 0 && MaximumLength < int.MaxValue)
            {
                yield return $"be between {MinimumLength} to {MaximumLength} characters";
            }

            if (RequireLower && !RequireUpper)
            {
                yield return "contain at least one lower case letter";
            }
            else if (RequireUpper && !RequireLower)
            {
                yield return "contain at least one upper case letter";
            }
            else if (RequireLower && RequireUpper)
            {
                yield return "contain at least one upper and one lower case letter";
            }

            if (RequireNumber)
            {
                yield return "contain at least one number";
            }

            if (RequireSymbol)
            {
                yield return "contain at least one symbol";
            }
        }
        #endregion
    }
}