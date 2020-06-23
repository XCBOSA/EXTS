using System.Collections.Generic;

namespace Island.StandardLib.SEXT
{
    public abstract class Statement
    {
        internal EXTSEngine engine;
        public Statement() => engine = EXTSEngine.CompilingEngine;
        public abstract string Eval();
    }

    public class StatementList : Statement
    {
        internal List<Statement> statements;
        internal Dictionary<string, string> val;

        public StatementList(params Statement[] sms)
        {
            statements = new List<Statement>(sms);
        }

        public void AddStatement(Statement statement)
        {
            statements.Add(statement);
        }

        public virtual void BeforeEval() { }

        public override string Eval()
        {
            val = new Dictionary<string, string>();
            BeforeEval();
            for (int i = 0; i < statements.Count; i++)
                statements[i].Eval();
            string ret = val.Get("return", "");
            val.Clear();
            return ret;
        }
    }

    public class SetValStatement : Statement
    {
        StatementList env;
        string name;
        Statement value;

        public SetValStatement(StatementList elist, string valname, Statement val)
        {
            env = elist;
            name = valname;
            value = val;
        }

        public override string Eval()
        {
            string val = value.Eval();
            env.val.Set(name, val);
            return val;
        }
    }

    public class GetValStatement : Statement
    {
        StatementList env;
        string name;

        public GetValStatement(StatementList elist, string valname)
        {
            env = elist;
            name = valname;
        }

        public override string Eval() => env.val.Get(name);
    }

    public class ImmediateStatement : Statement
    {
        string val;
        public ImmediateStatement(string immval) => val = immval;
        public override string Eval() => val;
    }

    public class FuncStatement : StatementList
    {
        internal string[] parameters;
        public FuncStatement(params Statement[] statements) : base(statements) { }

        public override void BeforeEval()
        {
            base.BeforeEval();
            if (parameters != null)
            {
                for (int i = 0; i < parameters.Length; i++)
                    val.Set("pmt" + i, parameters[i]);
            }
            parameters = null;
        }

        public string Eval(string[] pmts)
        {
            parameters = pmts;
            return base.Eval();
        }
    }

    public class BodyFuncStatement : StatementList
    {
        internal KeyValuePair<string, string>[] parameters;
        public BodyFuncStatement(params Statement[] statements) : base(statements) { }

        public override void BeforeEval()
        {
            base.BeforeEval();
            if (parameters != null)
            {
                for (int i = 0; i < parameters.Length; i++)
                    val.Set(parameters[i].Key, parameters[i].Value);
            }
            parameters = null;
        }

        public string Eval(params KeyValuePair<string, string>[] pmts)
        {
            parameters = pmts;
            return base.Eval();
        }
    }

    public class CallFuncStatement : Statement
    {
        string func;
        Statement[] pmts;

        public CallFuncStatement(string funcName, params Statement[] parameters)
        {
            for (int i = 0; i < parameters.Length; i++)
                parameters[i].engine = engine;
            func = funcName;
            pmts = parameters;
        }

        public override string Eval() => engine.RunFuncBase(func, pmts.Do((p) => p.Eval()));
    }
}
