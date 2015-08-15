﻿//MIT 2015, EngineKit

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

//---------------------------------------
//Warning
//concept/test only 
//-------------------------------------
//TODO: use ExpressionTree Walker  ***
//-------------------------------------

namespace SharpConnect.FluentQuery
{
    public delegate bool QueryPredicate<T>(T t);
    public delegate R QueryProduct<T, R>(T t);
    public delegate void ItemWalk<T>(T t);

    public abstract class QuerySegment
    {
        public QuerySegment PrevSegment { get; internal set; }
        public QuerySegment NextSegment { get; internal set; }
        public abstract QuerySegmentKind SegmentKind { get; }


        internal abstract void WriteToSelectStmt(SelectStatement selectStmt);
        internal abstract void WriteToInsertStmt(InsertStatement insertStmt);


        public CodeStatement MakeCodeStatement()
        {
            QuerySegmentParser parser = new QuerySegmentParser();
            return parser.Parse(this);
        }

    }

    public enum QuerySegmentKind
    {
        DataSource,
        Where,
        Select,
        Insert,
        Delete,
        Update,
    }

    abstract class ExpressionHolder
    {

        public abstract void WriteToSelectStatement(SelectStatement selectStmt);
        public abstract void WriteToInsertStatement(InsertStatement insertStmt);
    }

    class SelectProductHolder<S, R> : ExpressionHolder
    {
        Expression<QueryProduct<S, R>> product;
        public SelectProductHolder(Expression<QueryProduct<S, R>> product)
        {
            this.product = product;
        }
        public override void WriteToSelectStatement(SelectStatement selectStmt)
        {
            LinqExpressionTreeWalker exprWalker = new LinqExpressionTreeWalker();
            exprWalker.CreationContext = CreationContext.Select;
            exprWalker.Start(product.Body);

            SelectExpression selectExpr = new SelectExpression();
            selectExpr.selectClause = exprWalker.GetWalkResult();
            selectStmt.selectExpressions.Add(selectExpr);
        }
        public override void WriteToInsertStatement(InsertStatement insertStmt)
        {
            LinqExpressionTreeWalker exprWalker = new LinqExpressionTreeWalker();
            exprWalker.CreationContext = CreationContext.Insert;
            exprWalker.Start(product.Body);




        }
    }


    class QuerySegmentParser
    {
        public QuerySegmentParser()
        {

        }
        public CodeStatement Parse(QuerySegment segment)
        {
            //goto first segment 
            QuerySegment cNode = segment;
            while (cNode.PrevSegment != null)
            {
                cNode = cNode.PrevSegment;
            }
            //forward collect ..
            List<QuerySegment> nodeList = new List<QuerySegment>();
            while (cNode != null)
            {
                nodeList.Add(cNode);
                cNode = cNode.NextSegment;
            }

            //------------------------------------------------
            //select ...
            //from ...
            int j = nodeList.Count;
            QuerySegment fromSource = null;
            QuerySegment selectClause = null;
            QuerySegment insertClause = null;

            int state = 0;
            for (int i = 0; i < j; ++i)
            {
                var node = nodeList[i];
                switch (node.SegmentKind)
                {
                    case QuerySegmentKind.Select:
                        {

                            selectClause = node;
                        }
                        break;
                    case QuerySegmentKind.Where:
                    case QuerySegmentKind.DataSource:
                        {
                            fromSource = node;
                        }
                        break;
                    case QuerySegmentKind.Insert:
                        insertClause = node;
                        break;
                    default:
                        throw new NotSupportedException();
                }
            }
            //------------------------------------------------
            if (selectClause != null)
            {
                //create sql select
                SelectStatement selectStmt = new SelectStatement();
                //check data source
                if (fromSource != null)
                {
                    fromSource.WriteToSelectStmt(selectStmt);
                }
                selectClause.WriteToSelectStmt(selectStmt);
                return selectStmt;

            }
            else if (insertClause != null)
            {
                InsertStatement insertStatement = new InsertStatement();
                if (fromSource != null)
                {
                    fromSource.WriteToInsertStmt(insertStatement);
                }

                insertClause.WriteToInsertStmt(insertStatement);
                return insertStatement;

            }
            else
            {
                return null;
            }

        }
    }


    public class FromQry<S> : QuerySegment
    {


        QuerySegmentKind segmentKind;
        List<Expression<QueryPredicate<S>>> whereClauses = new List<Expression<QueryPredicate<S>>>();
        public FromQry()
        {
            this.segmentKind = QuerySegmentKind.DataSource;
        }

        public override QuerySegmentKind SegmentKind
        {
            get
            {
                return this.segmentKind;
            }
        }
        public FromQry<S> Where(Expression<QueryPredicate<S>> wherePred)
        {
            whereClauses.Add(wherePred);
            return this;
        }
        public SelectQry<R> Select<R>(Expression<QueryProduct<S, R>> product)
        {
            var q = new SelectQry<R>(this);
            q.exprHolder = new SelectProductHolder<S, R>(product);
            return q;
        }
        public SelectQry<R> SelectInto<R>()
        {
            return new SelectQry<R>(this);
        }

        internal override void WriteToSelectStmt(SelectStatement selectStmt)
        {
            FromExpression fromExpr = new FromExpression();
            fromExpr.dataSource = typeof(S).Name;
            selectStmt.fromExpressions.Add(fromExpr);
            //-------------------------------------------------
            int j = whereClauses.Count;
            if (j > 0)
            {
                //create where clause
                LinqExpressionTreeWalker walker = new LinqExpressionTreeWalker();
                walker.CreationContext = CreationContext.WhereClause;


                for (int i = 0; i < j; ++i)
                {
                    WhereExpression whereExpr = new WhereExpression();

                    var whereClause = whereClauses[i];
                    walker.Start(whereClause.Body);

                    whereExpr.whereClause = walker.GetWalkResult();
                    selectStmt.whereExpressions.Add(whereExpr);
                }
            }

        }
        internal override void WriteToInsertStmt(InsertStatement insertStmt)
        {
            insertStmt.targetTable = typeof(S).Name;

        }
    }


    public class InsertQry<S> : QuerySegment
    {
        internal ExpressionHolder exprHolder;
        public InsertQry()
        {
        }
        public InsertQry<S> Values(Expression<QueryProduct<S, S>> product)
        {
            exprHolder = new SelectProductHolder<S, S>(product);
            return this;
        }
        public InsertQry<S> Values<R>(Expression<QueryProduct<S, R>> product)
        {
            exprHolder = new SelectProductHolder<S, R>(product);
            return this;

        }
        public override QuerySegmentKind SegmentKind
        {
            get
            {
                return QuerySegmentKind.Insert;
            }
        }
        internal override void WriteToInsertStmt(InsertStatement insertStmt)
        {
            if (exprHolder != null)
            {
                exprHolder.WriteToInsertStatement(insertStmt);

            }
            else
            {
                throw new NotSupportedException();
            }
        }
        internal override void WriteToSelectStmt(SelectStatement selectStmt)
        {
            throw new NotImplementedException();
        }
    }

    public class SelectQry<S> : QuerySegment
    {
        int limit0 = -1;//default
        internal ExpressionHolder exprHolder;
        public SelectQry(QuerySegment prev)
        {

            this.PrevSegment = prev;
            prev.NextSegment = this;
        }

        /// <summary>
        /// mysql selection limit
        /// </summary>
        /// <param name="number"></param>
        public void Limit(int number)
        {
            limit0 = number;
        }


        public override QuerySegmentKind SegmentKind
        {
            get
            {
                return QuerySegmentKind.Select;
            }
        }

        internal override void WriteToSelectStmt(SelectStatement selectStmt)
        {
            if (exprHolder != null)
            {
                exprHolder.WriteToSelectStatement(selectStmt);
                selectStmt.limit0 = limit0;
            }
            else
            {
                throw new NotSupportedException();
            }
        }
        internal override void WriteToInsertStmt(InsertStatement insertStmt)
        {
            throw new NotSupportedException();
        }
    }

}