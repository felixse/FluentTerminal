using System;
using System.Collections.Generic;
using System.Linq;
using FluentTerminal.Models.Enums;

namespace FluentTerminal.Models
{
    public static class SshConnectionInfoValidationResultExtensions
    {
        public static string GetErrorString(this SshConnectionInfoValidationResult result, string separator = "; ") =>
            string.Join(separator, result.GetErrors());

        public static IEnumerable<string> GetErrors(this SshConnectionInfoValidationResult result)
        {
            if (result == SshConnectionInfoValidationResult.Valid)
            {
                yield break;
            }

            foreach (var value in Enum.GetValues(typeof(SshConnectionInfoValidationResult))
                .Cast<SshConnectionInfoValidationResult>().Where(r => r != SshConnectionInfoValidationResult.Valid))
            {
                if ((value & result) == value)
                {
                    yield return Resources.GetString($"{nameof(SshConnectionInfoValidationResult)}.{value}");
                }
            }
        }
    }
}