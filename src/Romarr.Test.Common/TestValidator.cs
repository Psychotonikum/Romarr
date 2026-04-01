using System;
using FluentValidation;

namespace Romarr.Test.Common
{
    public class TestValidator<T> : InlineValidator<T>
    {
        public TestValidator(params Action<TestValidator<T>>[] actions)
        {
            foreach (var action in actions)
            {
                action(this);
            }
        }
    }
}
