﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.CodeFixes.Async;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Test.Utilities;
using Xunit;

namespace Microsoft.CodeAnalysis.Editor.CSharp.UnitTests.Diagnostics.Async
{
    public partial class AddAwaitTests : AbstractCSharpDiagnosticProviderBasedUserDiagnosticTest
    {
        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsAddAwait)]
        public async Task BadAsyncReturnOperand1()
        {
            var initial =
@"using System;
using System.Threading.Tasks;

class Program
{
    async Task<int> Test()
    {
        return 3;
    }

    async Task<int> Test2()
    {
        [|return Test();|]
    }
}";

            var expected =
@"using System;
using System.Threading.Tasks;

class Program
{
    async Task<int> Test()
    {
        return 3;
    }

    async Task<int> Test2()
    {
        return await Test();
    }
}";
            await TestInRegularAndScriptAsync(initial, expected);
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsAddAwait)]
        public async Task BadAsyncReturnOperand_WithLeadingTrivia1()
        {
            var initial =
@"using System;
using System.Threading.Tasks;

class Program
{
    async Task<int> Test()
    {
        return 3;
    }

    async Task<int> Test2()
    {
        return
        // Useful comment
        [|Test()|];
    }
}";

            var expected =
@"using System;
using System.Threading.Tasks;

class Program
{
    async Task<int> Test()
    {
        return 3;
    }

    async Task<int> Test2()
    {
        return
        // Useful comment
        await Test();
    }
}";
            await TestInRegularAndScriptAsync(initial, expected);
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsAddAwait)]
        public async Task BadAsyncReturnOperand_ConditionalExpressionWithTrailingTrivia_SingleLine()
        {
            var initial =
@"using System;
using System.Threading.Tasks;

class Program
{
    async Task<int> Test() => 3;

    async Task<int> Test2()
    {[|
        return true ? Test() /* true */ : Test() /* false */;
    |]}
}";

            var expected =
@"using System;
using System.Threading.Tasks;

class Program
{
    async Task<int> Test() => 3;

    async Task<int> Test2()
    {
        return await (true ? Test() /* true */ : Test() /* false */);
    }
}";
            await TestInRegularAndScriptAsync(initial, expected);
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsAddAwait)]
        public async Task BadAsyncReturnOperand_ConditionalExpressionWithTrailingTrivia_Multiline()
        {
            var initial =
@"using System;
using System.Threading.Tasks;

class Program
{
    async Task<int> Test() => 3;

    async Task<int> Test2()
    {[|
        return true ? Test() // aaa
                    : Test() // bbb
                    ;
    |]}
}";

            var expected =
@"using System;
using System.Threading.Tasks;

class Program
{
    async Task<int> Test() => 3;

    async Task<int> Test2()
    {
        return await (true ? Test() // aaa
                    : Test()) // bbb
                    ;
    }
}";
            await TestInRegularAndScriptAsync(initial, expected);
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsAddAwait)]
        public async Task BadAsyncReturnOperand_NullCoalescingExpressionWithTrailingTrivia_SingleLine()
        {
            var initial =
@"using System;
using System.Threading.Tasks;

class Program
{
    async Task<int> Test() => 3;

    async Task<int> Test2()
    {[|
        return null /* 0 */ ?? Test() /* 1 */;
    |]}
}";

            var expected =
@"using System;
using System.Threading.Tasks;

class Program
{
    async Task<int> Test() => 3;

    async Task<int> Test2()
    {
        return await (null /* 0 */ ?? Test() /* 1 */);
    }
}";
            await TestInRegularAndScriptAsync(initial, expected);
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsAddAwait)]
        public async Task BadAsyncReturnOperand_NullCoalescingExpressionWithTrailingTrivia_Multiline()
        {
            var initial =
@"using System;
using System.Threading.Tasks;

class Program
{
    async Task<int> Test() => 3;

    async Task<int> Test2()
    {[|
        return null   // aaa
            ?? Test() // bbb
            ;
    |]}
}";

            var expected =
@"using System;
using System.Threading.Tasks;

class Program
{
    async Task<int> Test() => 3;

    async Task<int> Test2()
    {
        return await (null   // aaa
            ?? Test()) // bbb
            ;
    }
}";
            await TestInRegularAndScriptAsync(initial, expected);
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsAddAwait)]
        public async Task BadAsyncReturnOperand_AsExpressionWithTrailingTrivia_SingleLine()
        {
            var initial =
@"using System;
using System.Threading.Tasks;

class Program
{
    async Task<int> Test2()
    {[|
        return null /* 0 */ as Task<int> /* 1 */;
    |]}
}";

            var expected =
@"using System;
using System.Threading.Tasks;

class Program
{
    async Task<int> Test2()
    {
        return await (null /* 0 */ as Task<int> /* 1 */);
    }
}";
            await TestInRegularAndScriptAsync(initial, expected);
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsAddAwait)]
        public async Task BadAsyncReturnOperand_AsExpressionWithTrailingTrivia_Multiline()
        {
            var initial =
@"using System;
using System.Threading.Tasks;

class Program
{
    async Task<int> Test() => 3;

    async Task<int> Test2()
    {[|
        return null      // aaa
            as Task<int> // bbb
            ;
    |]}
}";

            var expected =
@"using System;
using System.Threading.Tasks;

class Program
{
    async Task<int> Test() => 3;

    async Task<int> Test2()
    {
        return await (null      // aaa
            as Task<int>) // bbb
            ;
    }
}";
            await TestInRegularAndScriptAsync(initial, expected);
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsAddAwait)]
        public async Task TaskNotAwaited()
        {
            var initial =
@"using System;
using System.Threading.Tasks;
class Program
{
    async void Test()
    {
        [|Task.Delay(3);|]
    }
}";

            var expected =
@"using System;
using System.Threading.Tasks;
class Program
{
    async void Test()
    {
        await Task.Delay(3);
    }
}";
            await TestInRegularAndScriptAsync(initial, expected);
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsAddAwait)]
        public async Task TaskNotAwaited_WithLeadingTrivia()
        {
            var initial =
@"using System;
using System.Threading.Tasks;
class Program
{
    async void Test()
    {

        // Useful comment
        [|Task.Delay(3);|]
    }
}";

            var expected =
@"using System;
using System.Threading.Tasks;
class Program
{
    async void Test()
    {

        // Useful comment
        await Task.Delay(3);
    }
}";
            await TestInRegularAndScriptAsync(initial, expected);
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsAddAwait)]
        public async Task FunctionNotAwaited()
        {
            var initial =
@"using System;
using System.Threading.Tasks;
class Program
{
    Task AwaitableFunction()
    {
        return Task.FromResult(true);
    }

    async void Test()
    {
        [|AwaitableFunction();|]
    }
}";

            var expected =
@"using System;
using System.Threading.Tasks;
class Program
{
    Task AwaitableFunction()
    {
        return Task.FromResult(true);
    }

    async void Test()
    {
        await AwaitableFunction();
    }
}";
            await TestInRegularAndScriptAsync(initial, expected);
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsAddAwait)]
        public async Task FunctionNotAwaited_WithLeadingTrivia()
        {
            var initial =
@"using System;
using System.Threading.Tasks;
class Program
{
    Task AwaitableFunction()
    {
        return Task.FromResult(true);
    }

    async void Test()
    {

        // Useful comment
        [|AwaitableFunction();|]
    }
}";

            var expected =
@"using System;
using System.Threading.Tasks;
class Program
{
    Task AwaitableFunction()
    {
        return Task.FromResult(true);
    }

    async void Test()
    {

        // Useful comment
        await AwaitableFunction();
    }
}";
            await TestInRegularAndScriptAsync(initial, expected);
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsAddAwait)]
        public async Task FunctionNotAwaited_WithLeadingTrivia1()
        {
            var initial =
@"using System;
using System.Threading.Tasks;
class Program
{
    Task AwaitableFunction()
    {
        return Task.FromResult(true);
    }

    async void Test()
    {
        var i = 0;

        [|AwaitableFunction();|]
    }
}";

            var expected =
@"using System;
using System.Threading.Tasks;
class Program
{
    Task AwaitableFunction()
    {
        return Task.FromResult(true);
    }

    async void Test()
    {
        var i = 0;

        await AwaitableFunction();
    }
}";
            await TestInRegularAndScriptAsync(initial, expected);
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsAddAwait)]
        public async Task TestAssignmentExpression()
        {
            await TestInRegularAndScriptAsync(
@"using System.Threading.Tasks;

class TestClass
{
    private async Task MyTestMethod1Async()
    {
        int myInt = [|MyIntMethodAsync()|];
    }

    private Task<int> MyIntMethodAsync()
    {
        return Task.FromResult(result: 1);
    }
}",
@"using System.Threading.Tasks;

class TestClass
{
    private async Task MyTestMethod1Async()
    {
        int myInt = await MyIntMethodAsync();
    }

    private Task<int> MyIntMethodAsync()
    {
        return Task.FromResult(result: 1);
    }
}");
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsAddAwait)]
        public async Task TestAssignmentExpressionWithConversion()
        {
            await TestInRegularAndScriptAsync(
@"using System.Threading.Tasks;

class TestClass
{
    private async Task MyTestMethod1Async()
    {
        long myInt = [|MyIntMethodAsync()|];
    }

    private Task<int> MyIntMethodAsync()
    {
        return Task.FromResult(result: 1);
    }
}",
@"using System.Threading.Tasks;

class TestClass
{
    private async Task MyTestMethod1Async()
    {
        long myInt = await MyIntMethodAsync();
    }

    private Task<int> MyIntMethodAsync()
    {
        return Task.FromResult(result: 1);
    }
}");
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsAddAwait)]
        public async Task TestAssignmentExpressionWithConversionInNonAsyncFunction()
        {
            await TestMissingInRegularAndScriptAsync(
@"using System.Threading.Tasks;

class TestClass
{
    private Task MyTestMethod1Async()
    {
        long myInt = [|MyIntMethodAsync()|];
    }

    private Task<int> MyIntMethodAsync()
    {
        return Task.FromResult(result: 1);
    }
}");
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsAddAwait)]
        public async Task TestAssignmentExpressionWithConversionInAsyncFunction()
        {
            await TestInRegularAndScriptAsync(
@"using System.Threading.Tasks;

class TestClass
{
    private async Task MyTestMethod1Async()
    {
        long myInt = [|MyIntMethodAsync()|];
    }

    private Task<object> MyIntMethodAsync()
    {
        return Task.FromResult(new object());
    }
}",
@"using System.Threading.Tasks;

class TestClass
{
    private async Task MyTestMethod1Async()
    {
        long myInt = await MyIntMethodAsync();
    }

    private Task<object> MyIntMethodAsync()
    {
        return Task.FromResult(new object());
    }
}");
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsAddAwait)]
        public async Task TestAssignmentExpression1()
        {
            await TestInRegularAndScriptAsync(
@"using System;
using System.Threading.Tasks;

class TestClass
{
    private async Task MyTestMethod1Async()
    {
        Action lambda = async () => {
            int myInt = [|MyIntMethodAsync()|];
        };
    }

    private Task<int> MyIntMethodAsync()
    {
        return Task.FromResult(result: 1);
    }
}",
@"using System;
using System.Threading.Tasks;

class TestClass
{
    private async Task MyTestMethod1Async()
    {
        Action lambda = async () => {
            int myInt = await MyIntMethodAsync();
        };
    }

    private Task<int> MyIntMethodAsync()
    {
        return Task.FromResult(result: 1);
    }
}");
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsAddAwait)]
        public async Task TestAssignmentExpression2()
        {
            await TestInRegularAndScriptAsync(
@"using System;
using System.Threading.Tasks;

class TestClass
{
    private async Task MyTestMethod1Async()
    {
        Func<Task> lambda = async () => {
            int myInt = [|MyIntMethodAsync()|];
        };
    }

    private Task<int> MyIntMethodAsync()
    {
        return Task.FromResult(result: 1);
    }
}",
@"using System;
using System.Threading.Tasks;

class TestClass
{
    private async Task MyTestMethod1Async()
    {
        Func<Task> lambda = async () => {
            int myInt = await MyIntMethodAsync();
        };
    }

    private Task<int> MyIntMethodAsync()
    {
        return Task.FromResult(result: 1);
    }
}");
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsAddAwait)]
        public async Task TestAssignmentExpression3()
        {
            await TestMissingInRegularAndScriptAsync(
@"using System;
using System.Threading.Tasks;

class TestClass
{
    private async Task MyTestMethod1Async()
    {
        Func<Task> lambda = () => {
            int myInt = MyInt [||] MethodAsync();
        };
    }

    private Task<int> MyIntMethodAsync()
    {
        return Task.FromResult(result: 1);
    }
}");
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsAddAwait)]
        public async Task TestAssignmentExpression4()
        {
            await TestMissingInRegularAndScriptAsync(
@"using System;
using System.Threading.Tasks;

class TestClass
{
    private async Task MyTestMethod1Async()
    {
        Action lambda = () => {
            int myInt = MyIntM [||] ethodAsync();
        };
    }

    private Task<int> MyIntMethodAsync()
    {
        return Task.FromResult(result: 1);
    }
}");
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsAddAwait)]
        public async Task TestAssignmentExpression5()
        {
            await TestInRegularAndScriptAsync(
@"using System;
using System.Threading.Tasks;

class TestClass
{
    private async Task MyTestMethod1Async()
    {
        Action @delegate = async delegate {
            int myInt = [|MyIntMethodAsync()|];
        };
    }

    private Task<int> MyIntMethodAsync()
    {
        return Task.FromResult(result: 1);
    }
}",
@"using System;
using System.Threading.Tasks;

class TestClass
{
    private async Task MyTestMethod1Async()
    {
        Action @delegate = async delegate {
            int myInt = await MyIntMethodAsync();
        };
    }

    private Task<int> MyIntMethodAsync()
    {
        return Task.FromResult(result: 1);
    }
}");
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsAddAwait)]
        public async Task TestAssignmentExpression6()
        {
            await TestInRegularAndScriptAsync(
@"using System;
using System.Threading.Tasks;

class TestClass
{
    private async Task MyTestMethod1Async()
    {
        Func<Task> @delegate = async delegate {
            int myInt = [|MyIntMethodAsync()|];
        };
    }

    private Task<int> MyIntMethodAsync()
    {
        return Task.FromResult(result: 1);
    }
}",
@"using System;
using System.Threading.Tasks;

class TestClass
{
    private async Task MyTestMethod1Async()
    {
        Func<Task> @delegate = async delegate {
            int myInt = await MyIntMethodAsync();
        };
    }

    private Task<int> MyIntMethodAsync()
    {
        return Task.FromResult(result: 1);
    }
}");
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsAddAwait)]
        public async Task TestAssignmentExpression7()
        {
            await TestMissingInRegularAndScriptAsync(
@"using System;
using System.Threading.Tasks;

class TestClass
{
    private async Task MyTestMethod1Async()
    {
        Action @delegate = delegate {
            int myInt = MyInt [||] MethodAsync();
        };
    }

    private Task<int> MyIntMethodAsync()
    {
        return Task.FromResult(result: 1);
    }
}");
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsAddAwait)]
        public async Task TestAssignmentExpression8()
        {
            await TestMissingInRegularAndScriptAsync(
@"using System;
using System.Threading.Tasks;

class TestClass
{
    private async Task MyTestMethod1Async()
    {
        Func<Task> @delegate = delegate {
            int myInt = MyIntM [||] ethodAsync();
        };
    }

    private Task<int> MyIntMethodAsync()
    {
        return Task.FromResult(result: 1);
    }
}");
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsAddAwait)]
        public async Task TestTernaryOperator()
        {
            await TestInRegularAndScriptAsync(
@"using System;
using System.Threading.Tasks;

class Program
{
    async Task<int> A()
    {
        return [|true ? Task.FromResult(0) : Task.FromResult(1)|];
    }
}",
@"using System;
using System.Threading.Tasks;

class Program
{
    async Task<int> A()
    {
        return await (true ? Task.FromResult(0) : Task.FromResult(1));
    }
}");
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsAddAwait)]
        public async Task TestNullCoalescingOperator()
        {
            await TestInRegularAndScriptAsync(
@"using System;
using System.Threading.Tasks;

class Program
{
    async Task<int> A()
    {
        return [|null ?? Task.FromResult(1)|] }
}",
@"using System;
using System.Threading.Tasks;

class Program
{
    async Task<int> A()
    {
        return await (null ?? Task.FromResult(1))}
}");
        }

        [Fact, Trait(Traits.Feature, Traits.Features.CodeActionsAddAwait)]
        public async Task TestAsExpression()
        {
            await TestInRegularAndScriptAsync(
@"using System;
using System.Threading.Tasks;

class Program
{
    async Task<int> A()
    {
        return [|null as Task<int>|] }
}",
@"using System;
using System.Threading.Tasks;

class Program
{
    async Task<int> A()
    {
        return await (null as Task<int>)}
}");
        }

        internal override (DiagnosticAnalyzer, CodeFixProvider) CreateDiagnosticProviderAndFixer(Workspace workspace)
            => (null, new CSharpAddAwaitCodeFixProvider());
    }
}
