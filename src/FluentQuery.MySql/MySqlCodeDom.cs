﻿//MIT 2015, EngineKit

using System;
using System.Collections.Generic;
using System.Text;

//---------------------------------------
//Warning
//concept/test only 
//-------------------------------------

namespace SharpConnect.FluentQuery
{

    public abstract class CodeExpression
    {

    }
    public abstract class CodeStatement
    {

    }
    public class SelectStatement : CodeStatement
    {
        public List<FromExpression> fromExpressions = new List<FromExpression>();
        public List<SelectExpression> selectExpressions = new List<SelectExpression>();
        public List<WhereExpression> whereExpressions = new List<WhereExpression>();

    }

    public class WhereExpression : CodeExpression
    {
        public string whereClause;

    }

    public class FromExpression : CodeExpression
    {
        public string dataSource;

    }
    public class SelectExpression : CodeExpression
    {
        public string selectClause;
    }

    public static class MySqlStringMaker
    {
        public static string BuildMySqlString(SelectStatement selectStmt)
        {
            StringBuilder stbuilder = new StringBuilder();

            stbuilder.Append("select ");
            int j = selectStmt.selectExpressions.Count;
            if (j > 0)
            {
                stbuilder.Append(selectStmt.selectExpressions[0].selectClause);
            }
            j = selectStmt.fromExpressions.Count;
            if (j > 0)
            {
                stbuilder.Append(" from ");
                stbuilder.Append(selectStmt.fromExpressions[0].dataSource);
            }
            j = selectStmt.whereExpressions.Count;
            if (j > 0)
            {
                stbuilder.Append(" where ");
                stbuilder.Append(selectStmt.whereExpressions[0].whereClause);
            }

            return stbuilder.ToString();
        }
    }

}