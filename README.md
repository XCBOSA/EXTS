# EXTS
Extension String
一个基于C#的、即时编译运行的脚本语言。此外，它还有基于C++和Swift的高效能版本。EXTS不依赖反射机制，它可以在任何AOT编译平台上运行 (如：iOS 热更新)
<br>
Island.StandardLib 是一个封装我大部分【轮子】的库，这个项目用到了一点点这里的内容，编译的话实现一下没有的函数就行啦~
### 基础语法
#### 一般变量赋值语句
```
valName = $123;
valName = "abc";
```
EXTS不需要事先声明变量，可直接开始使用。valName 是这个变量的名字，等号后面的内容是值。值可以是立即数 $123，也可以是立即字符串 "abc"，也可以是函数调用。在字符串中，支持\n \t \\" \\ 四个转义符。
<br>注意：EXTS需要在每个标准语句后面接 ; 表示语句结束。同一行可以有多个语句。
#### 函数调用语句
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
#### 函数定义语句
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
#### 全局变量 局部变量 和 变量的作用域
##### 全局变量
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

