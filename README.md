# EXTS
###### Extension String
一个基于C#的、即时编译运行的脚本语言。此外，它还有基于C++和Swift的高效能版本。EXTS不依赖反射机制，它可以在任何AOT编译平台上运行 (如：iOS 热更新)
<br>
Island.StandardLib 是一个封装我大部分【轮子】的库，这个项目用到了一点点这里的内容，编译的话实现一下没有的函数就行啦~

### 基础语法
#### 1. 一般变量赋值语句
```
valName = $123;
valName = "abc";
```
EXTS不需要事先声明变量，可直接开始使用。valName 是这个变量的名字，等号后面的内容是值。值可以是立即数 $123，也可以是立即字符串 "abc"，也可以是函数调用。在字符串中，支持\n \t \\" \\ 四个转义符。
<br>注意：EXTS需要在每个标准语句后面接 ; 表示语句结束。同一行可以有多个语句。

#### 2. 函数调用语句
```
[cprint];                              // 输出一个换行符
[cprint strA];                         // 输出变量 strA 的值
[cprint "Hello xc"];                   // 输出立即字符串 "Hello xc"
[cprint [strcombine strA strB]];       // 输出 strA 和 strB 拼接后的结果
```
在EXTS的函数调用语句中，每个函数调用由一组匹配的方括号括起来，每组方括号中的第一项为函数名称，后面是参数列表。其中，每个参数可以是立即数和立即字符串，也可以是某个变量名，也可以是一个嵌套的函数调用语句。比如：
```
[cprint [strcombine strA strB]];
```
```
valName = [strcombine strA strB];
[cprint valName];
```
是等效的

#### 3. 函数定义语句
##### 3.1 一般函数定义语句
```
func funcName: arg0 arg1
{
    [cprint [strcombine arg0 arg1]];
}
[funcName "Hello " "world"];
```
在EXTS中，函数定义语句由 `func 函数名 : 参数列表 { 函数体 } ` 组成。其中，当函数没有参数的时候，函数可简写为 `func 函数名 { 函数体 }` 的形式，例如：
```
func noParamFuncName
{
    [cprint "Hello"];
}
[noParamFuncName];
```
需要注意的是，不同的函数间的变量是完全隔离的。如果你想在 funcA 中访问 funcB 中的值，需要使用全局变量。

##### 3.2 嵌套函数
在EXTS中，允许函数嵌套。参考如下代码：
```
func main
{
    [cprint "Hello"];
    [.inner];
    [main.inner];
    func inner
    {
        [cprint " xc"];
    }
}
[main];
```
在这里，main 函数中嵌套了一个 inner 函数，inner 函数的实际名称是 main.inner ，通过这个名字可以在任何地方调用它，但是如果在 main 函数中，可以简化调用为 .inner 。
依此性质，这段代码将输出 Hello xc xc 。

#### 4. 变量的作用域
##### 4.1 全局变量
```
[static "staticVarA" value];             // 将名叫 "staticVarA" 的全局变量，赋值为 value。
localVarA = [static "staticVarA"];       // 取出全局变量 "staticVarA"，存储到局部变量 localVarA 中。
```
在这里，你可以注意到，static是一个函数，而它的参数列表有两种：
```
func static: strVarName { ... }          // 获取 strVarName
func static: strVarName valValue { ... } // 设置 strVarName 为 valValue
```
所以，要注意的是，设置和取用全局变量的时候，由于变量名是一个立即字符串，要加引号。当然，你也可以用局部变量中的值作为全局变量名，比如：
```
val valName = "xc";
[static valName "nb"];
[cprint [static valName]];               // 输出 "nb"
```

##### 4.2 变量的作用域
```
var_a = "ok";
func main
{
    var_b = "ok";
    [inner];
    func inner
    {
        var_c = "ok";
        [cprint var_a " " var_b " " var_c];      // 输出 non ok ok
    }
    [cprint var_a " " var_b " " var_c];          // 输出 non ok non
}
[cprint var_a " " var_b " " var_c];              // 输出 ok non non
[main];
```
总结一下，变量的作用域具有如下规律：<br>
1. 在最外侧、不属于任何函数的地方定义的变量，无法被函数内访问。<br>
2. 函数内部定义的变量，自身函数和函数中的子级函数可以访问，但它的父级无法访问。<br>
3. 当一个函数运行结束后，函数内的所有变量会被释放。

#### 5. 匿名函数和常函数
##### 5.1 匿名函数和它的函数指针
首先，我们定义函数 funcptr 如下：
```
func funcptr: in_func { return = in_func; }
```
依据第三章的知识，我们直到这个函数的作用是 输入什么值就输出什么值，比如：
```
val = [funcptr "xc"];
[cprint val];             //输出 xc
```
在这里，我们定义一类特殊的函数参数，它叫 <b>匿名函数</b> ，具有如下格式：
```
:{ 匿名函数体 };
:arg { 匿名函数体 };
```
它们具有和函数相同的性质，可以包含一句或多句代码，也可以换行。比如：
```
ptr = [funcptr :{
    [cprint "I am a anonymous function!"];
}];
```
但是，我们直到，要访问一个函数，必须使用它的完全限定或部分限定名称，比如 [main] [.inner]; 等等。匿名函数不具有函数名称，那么需要使用函数指针来访问它。<br>
我们的 匿名函数参数，默认情况下返回一个4字节(2x2)的指针，由 funcptr 的定义可知，ptr就是这个匿名函数参数返回的指针。<br>
如果我们需要调用这个匿名函数，只需要：
```
[callfuncptr ptr];
```
callfuncptr函数也具有返回值，它返回 ptr 指向的匿名函数调用后的返回值。比如：
```
ptr = [funcptr :arg0 arg1{
    return = [strcombine arg0 arg1];
}];
a = [callfuncptr ptr "xc" "nb"];
[cprint a];                             // 输出 xcnb
```
##### 5.2 匿名函数和常函数的本质区别
本节介绍常函数（具有名字的函数）和匿名函数的本质区别。在本节中，<b>编译</b>的含义是 EXTSEngine::Compile() ，即进行从上到下语法分析的过程。您应始终明确，EXTS作为脚本语言，代码无需真正编译即可运行。
```
func ptr: a { return = a; }
func main
{
    func inner
    {
        [cprint "Hello "];
    }
    a = [ptr :{
        [cprint "xc"];
    }];
    [.inner];
    [callfuncptr a];
}
[main];
```
在EXTS中，规定具有名称的函数为常函数，无名称的函数为匿名函数，匿名函数只能出现在函数调用参数中。<br>
在编译过程中，常函数将编译到常函数存储区，它是在编译时确定的，匿名函数是在运行中确定的。也就是说，你可以在文档的任何位置通过 [func]; 来调用一个常函数，无论是在 func 函数实现的上部还是下部。对于一个匿名函数，你只能在执行它的定义语句执行后调用它，因为在此之前它并不在运行时内存中。同样的，匿名函数的本机实现不依赖反射和任何不可AOT的机制，您可以在任何场景下放心使用。

