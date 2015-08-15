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

    /// <summary>
    /// query helper
    /// </summary>
    public static class Q
    {
        public static FromQry<T> From<T>()
        {
            return new FromQry<T>();
        }

        public static FromQry<T> From<T>(T s)
        {
            return new FromQry<T>();
        }
        /// <summary>
        /// insert into
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static InsertQry<T> InsertInto<T>()
        {
            return new InsertQry<T>();
        }
        /// <summary>
        /// insert into
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static UpdateQry<T> Update<T>()
        {
            return new UpdateQry<T>();
        }


    }

    public static class From<T>
    {

        public static FromQry<T> Where(Expression<QueryPredicate<T>> pred)
        {
            FromQry<T> fromQ = new FromQry<T>();
            fromQ.Where(pred);
            return fromQ;
        }

    }
    public static class Update<T>
    {
        public static UpdateQry<T> Where(Expression<QueryPredicate<T>> pred)
        {
            UpdateQry<T> updateQ = new UpdateQry<T>();
            updateQ.Where(pred);
            return updateQ;
        }
        public static UpdateQry<T> Set<TResult>(Expression<QueryProduct<T, TResult>> setClause)
        {
            UpdateQry<T> updateQ = new UpdateQry<T>();
            updateQ.Set(setClause);
            return updateQ;

        }
    }
    public static class InsertInto<T>
    {
        public static InsertQry<T> Values<TResult>(Expression<QueryProduct<T, TResult>> setClause)
        {

            InsertQry<T> insertQ = new InsertQry<T>();
            insertQ.Values(setClause);
            return insertQ;

        }
    }
}