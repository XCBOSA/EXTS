using System;
using System.Collections.Generic;

namespace Island.StandardLib.SEXT
{
    public class EXTSEngine
    {
        internal static EXTSEngine CompilingEngine;

        public const string ValError = "err";
        public const string ValDefault = "non";

        public Dictionary<string, string> stdval;
        public Dictionary<string, FuncStatement> funcs;
        public BodyFuncStatement BodyStatement;

        public EXTSEngine()
        {
            stdval = new Dictionary<string, string>();
            funcs = new Dictionary<string, FuncStatement>();
            BodyStatement = new BodyFuncStatement(new Statement[0]);
        }

        public T AddStatement<T>(T statement) where T : Statement
        {
            BodyStatement.statements.Add(statement);
            return statement;
        }

        public FuncStatement AddStatement(string name, FuncStatement statement)
        {
            funcs.Add(name, statement);
            return statement;
        }

        internal Stack<string> func_invoke_stack;

        public string RunFuncBase(string funcName, string[] parameters)
        {
            if (funcName == "") return parameters.Length > 0 ? parameters[0] : "";
            switch (funcName)
            {
                case "strcombine":
                    {
                        string str = "";
                        for (int i = 0; i < parameters.Length; i++)
                            str += parameters[i];
                        return str;
                    }

                case "cprint":
                    {
                        string str = "";
                        for (int i = 0; i < parameters.Length; i++)
                            str += parameters[i];
                        Console.WriteLine(str);
                        return str;
                    }

                case "cread":
                    {
                        return Console.ReadLine();
                    }

                case "static":
                    {
                        if (parameters.Length == 1)
                            return stdval.Get(parameters[0], ValDefault);
                        else if (parameters.Length > 1)
                        {
                            string str = "";
                            for (int i = 1; i < parameters.Length; i++)
                                str += parameters[i];
                            stdval.Set(parameters[0], str);
                            return str;
                        }
                        else return ValError;
                    }

                case "stackinfo":
                    {
                        return StkName(func_invoke_stack, true, " -> ");
                    }

                default:
                    {
                        FuncStatement fs = funcs.Get(funcName);
                        if (fs == null)
                            return ValError;
                        func_invoke_stack.Push(funcName);
                        string ret = fs.Eval(parameters);
                        func_invoke_stack.Pop();
                        return ret;
                    }
            }
        }

        string compilingCode;
        int compilingPos;
        const char EOF = (char)0;

        bool InList(char ch, char[] add)
        {
            for (int i = 0; i < add.Length; i++)
                if (ch == add[i]) return true;
            return false;
        }

        bool ChBlank(char ch)
        {
            if (ch == ' ' || ch == '\t' || ch == '\n') return true;
            else return false;
        }

        bool ChBlankTEOFA(char ch, char[] add)
        {
            if (ch == ' ' || ch == '\t' || ch == '\n' || ch == EOF || ch == ']') return true;
            return InList(ch, add);
        }

        char Peek()
        {
            if (compilingPos < compilingCode.Length)
                return compilingCode[compilingPos++];
            else
            {
                compilingPos++;
                return EOF;
            }
        }

        const string symallowed = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789_";

        bool SymAllowed(char c, bool loc)
        {
            if (loc && c == '.') return true;
            for (int i = 0; i < symallowed.Length; i++)
                if (c == symallowed[i]) return true;
            return false;
        }

        /// <summary>
        /// 读取当前位置的字符，直到遇到空白字符和 excludelist 中的字符
        /// </summary>
        /// <param name="excludelist">被识别为空白字符的附加字符</param>
        /// <returns></returns>
        string PeekToBlank(params char[] excludelist)
        {
            string str = "";
            char ch;
            while (!ChBlankTEOFA(ch = Peek(), excludelist)) str += ch;
            compilingPos--;
            return str;
        }

        /// <summary>
        /// 读取当前位置的字符，直到遇到空白字符和 excludelist 中的字符，只允许字母数字和下划线输入
        /// </summary>
        /// <param name="excludelist">被识别为空白字符的附加字符</param>
        /// <returns></returns>
        string PeekToBlankSym(params char[] excludelist)
        {
            string str = "";
            char ch;
            while (!ChBlankTEOFA(ch = Peek(), excludelist))
            {
                if (!SymAllowed(ch, false)) throw new SyntaxException("在符号定义中，不允许出现字符 " + ch, compilingPos);
                str += ch;
            }
            compilingPos--;
            return str;
        }

        string PeekToBlankLocSym(params char[] excludelist)
        {
            string str = "";
            char ch;
            while (!ChBlankTEOFA(ch = Peek(), excludelist))
            {
                if (!SymAllowed(ch, true)) throw new SyntaxException("在符号定义中，不允许出现字符 " + ch, compilingPos);
                str += ch;
            }
            compilingPos--;
            return str;
        }

        /// <summary>
        /// 跳过空白字符和 includelist 中的字符，转到下一个字的前一个位置
        /// </summary>
        /// <param name="includelist">被识别为空白字符的附加字符</param>
        void PeekToWord(params char[] includelist)
        {
            char ch;
            while (ChBlank(ch = Peek()) || InList(ch, includelist)) ;
            compilingPos--;
        }

        string PeekString()
        {
            string str = "";
            char ch;
            while (true)
            {
                ch = Peek();
                if (ch == '\\')
                {
                    char ct = Peek();
                    switch (ct)
                    {
                        case 'n': str += '\n'; break;
                        case 't': str += '\t'; break;
                        case '\"': str += '\"'; break;
                        case '\\': str += '\\'; break;
                        default: throw new SyntaxException("未识别的转义符。", compilingPos);
                    }
                }
                if (ch == EOF) throw new SyntaxException("字符串直到文件结尾都未结束，请检查引号是否完整。", compilingPos);
                if (ch == '\"')
                {
                    break;
                }
                str += ch;
            }
            return str;
        }

        ImmediateStatement CompileImmediateStatementF()
        {
            ImmediateStatement statement = new ImmediateStatement(PeekToBlank());
            PeekToWord();
            return statement;
        }

        ImmediateStatement CompileImmediateStatementS()
        {
            ImmediateStatement statement = new ImmediateStatement(PeekString());
            PeekToWord();
            return statement;
        }

        GetValStatement CompileGetValStatement()
        {
            GetValStatement statement = new GetValStatement(currentfunc, PeekToBlankSym());
            PeekToWord();
            return statement;
        }

        CallFuncStatement CompileCallFuncStatement()
        {
            List<Statement> pmts = new List<Statement>();
            PeekToWord();
            string calName = PeekToBlankLocSym('[');
            PeekToWord();
            while (true)
            {
                switch (Peek())
                {
                    case '[': pmts.Add(CompileCallFuncStatement()); break;
                    case '$': pmts.Add(CompileImmediateStatementF()); break;
                    case '\"': pmts.Add(CompileImmediateStatementS()); break;
                    case ']': return new CallFuncStatement(calName, pmts.ToArray());
                    case ';': throw new SyntaxException("在函数调用中，意外的语句结束。", compilingPos);
                    case EOF: throw new SyntaxException("函数调用直到文件结尾都未结束，请检查方括号是否匹配。", compilingPos);
                    default: compilingPos--; pmts.Add(CompileGetValStatement()); break;
                }
                PeekToWord();
            }
        }

        SetValStatement CompileSetValStatement(string name)
        {
            PeekToWord();
            Statement valst;
            switch (Peek())
            {
                case '[': valst = CompileCallFuncStatement(); break;
                case '$': valst = CompileImmediateStatementF(); break;
                case '\"': valst = CompileImmediateStatementS(); break;
                case ']': throw new SyntaxException("在赋值语句中，意外的符号 ]。", compilingPos);
                case ';': throw new SyntaxException("在赋值语句中，意外的语句结束。", compilingPos);
                case EOF: throw new SyntaxException("赋值语句直到文件结尾都未结束，请检查方括号是否匹配。", compilingPos);
                default: valst = CompileGetValStatement(); break;
            }
            PeekToWord();
            char ch = Peek();
            if (ch == ';') return new SetValStatement(currentfunc, name, valst);
            throw new SyntaxException("赋值语句结束后仍然出现语句，请检查是否缺少分号。", compilingPos);
        }

        string StkName(Stack<string> stk, bool reverse, string link = ".")
        {
            string b = "";
            List<string> r = new List<string>();
            foreach (string st in stk)
                r.Add(st);
            if (reverse) r.Reverse();
            for (int i = 0; i < r.Count; i++)
                b += r[i] + link;
            if (b.Length != 0)
                b = b.Substring(0, b.Length - link.Length);
            return b;
        }

        FuncStatement CompileFuncStatement()
        {
            string funcname = PeekToBlankSym(':', '{');
            FuncStatement func = new FuncStatement();
            currentfunc = func;
            func_compile_stack.Push(funcname);
            string fullfuncname = StkName(func_compile_stack, true);
            PeekToWord();
            char t = Peek();
            if (t == ':')
            {
                int i = 0;
                while (true)
                {
                    PeekToWord();
                    char p = Peek();
                    if (p == EOF) throw new SyntaxException("函数的参数列表直到文件结尾都未结束，请检查函数 " + funcname + " 的定义。", compilingPos);
                    if (p == '{') break;
                    compilingPos--;
                    string varname = PeekToBlankSym();
                    func.AddStatement(new SetValStatement(func, varname, new GetValStatement(func, "pmt" + i)));
                    i++;
                }
                PeekToWord();
            }
            else if (t == '{')
                PeekToWord();
            else throw new SyntaxException("错误的函数表达式形式。", compilingPos);

            char ch;
            while (true)
            {
                PeekToWord();
                ch = Peek();
                switch (ch)
                {
                    case EOF:
                        throw new SyntaxException("函数 " + funcname + " 直到文件结尾都未结束，请检查大括号是否匹配。", compilingPos);
                    case '}':
                        funcs.Add(fullfuncname, func);
                        func_compile_stack.Pop();
                        return func;
                    case '[':
                        func.AddStatement(CompileCallFuncStatement());
                        PeekToWord();
                        if (Peek() != ';') throw new SyntaxException("函数调用结束后仍然出现语句，请检查是否缺少分号。", compilingPos);
                        break;
                    default:
                        {
                            string name = ch + PeekToBlankSym('=');
                            PeekToWord();
                            char p = Peek();
                            if (p == EOF) throw new SyntaxException("不可分析的文件结尾。", compilingPos);
                            else if (p == '=') func.AddStatement(CompileSetValStatement(name));
                            else
                            {
                                compilingPos--;
                                if (name == "func")
                                {
                                    CompileFuncStatement();
                                    currentfunc = func;
                                }
                            }
                        }
                        break;
                }
            }
        }

        StatementList currentfunc;
        Stack<string> func_compile_stack;

        public void Compile(string str)
        {
            str = str.Replace("\0", "").Replace("\r", "");
            CompilingEngine = this;
            compilingCode = str;
            compilingPos = 0;
            currentfunc = BodyStatement;
            func_compile_stack = new Stack<string>();
            char ch;
            while (true)
            {
                PeekToWord();
                ch = Peek();
                switch (ch)
                {
                    case EOF:
                        return;
                    case '[':
                        currentfunc.AddStatement(CompileCallFuncStatement());
                        PeekToWord();
                        if (Peek() != ';') throw new SyntaxException("函数调用结束后仍然出现语句，请检查是否缺少分号。", compilingPos);
                        break;
                    default:
                        {
                            string name = ch + PeekToBlankSym('=');
                            PeekToWord();
                            char p = Peek();
                            if (p == EOF) throw new SyntaxException("不可分析的文件结尾。", compilingPos);
                            else if (p == '=') currentfunc.AddStatement(CompileSetValStatement(name));
                            else
                            {
                                compilingPos--;
                                if (name == "func")
                                {
                                    CompileFuncStatement();
                                    currentfunc = BodyStatement;
                                }
                            }
                        }
                        break;
                }
            }
        }

        public void Reset()
        {
            BodyStatement = new BodyFuncStatement();
        }

        public string Run(params KeyValuePair<string, string>[] pmts)
        {
            stdval.Clear();
            func_invoke_stack = new Stack<string>();
            return BodyStatement.Eval(pmts);
        }
    }
}
