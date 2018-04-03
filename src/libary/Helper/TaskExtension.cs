using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace pillowsharp.Helper
{
    public static class TaskExtension
    {
        public static async Task Then(
    this Task antecedent, Action continuation)
        {
            await antecedent;
            continuation();
        }

        public static async Task<TNewResult> Then<TNewResult>(
            this Task antecedent, Func<TNewResult> continuation)
        {
            await antecedent;
            return continuation();
        }

        public static async Task Then<TResult>(
            this Task<TResult> antecedent, Action<TResult> continuation)
        {
            continuation(await antecedent);
        }

        public static async Task<TNewResult> Then<TResult, TNewResult>(
            this Task<TResult> antecedent, Func<TResult, TNewResult> continuation)
        {
            return continuation(await antecedent);
        }
    }
}
