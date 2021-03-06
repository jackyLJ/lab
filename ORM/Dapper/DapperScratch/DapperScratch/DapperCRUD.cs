﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Dapper;

namespace DapperScratch
{
    public class DapperCRUD
    {
        IDbConnection connection = new SqlConnection("Data Source=.;Initial Catalog=DapperLab;Integrated Security=True;MultipleActiveResultSets=True");

        public int SingleInsert(string userName, string email, string address)
        {
            var affectedRows = connection.Execute("insert into Users values (@UserName, @Email, @Address)",
                                           new { UserName = userName, Email = email, Address = address });

            return affectedRows;
        }

        public int SingleInsertProduct(string name, string description, DateTime createTime, int? userId = null)
        {
            var affectedRows = connection.Execute("insert into Product values (@ProductName, @ProductDesc, @UserID, @CreateTime)",
                new { ProductName = name, ProductDesc = description, UserID = userId, CreateTime = createTime });

            return affectedRows;
        }

        public int BulkInsert(IEnumerable<Users> newUsers)
        {
            return connection.Execute("insert into Users values (@UserName, @Email, @Address)", newUsers);
        }

        public IEnumerable<Users> QueryByUserName(string userName)
        {
            var result = connection.Query<Users>("select * from Users where UserName=@UserName",
                new { UserName = userName });

            return result;
        }

        public IEnumerable<Users> SearchByUserName(string userName)
        {
            var result = connection.Query<Users>("select * from Users where UserName like @UserName",
                new { UserName = userName + "%" });

            return result;
        }

        public void UpdateUser(int userId, string userName)
        {
            connection.Execute("update Users set UserName=@UserName where UserID=@UserID", new { UserID = userId, UserName = userName });
        }

        public void Delete(int userId)
        {
            connection.Execute("delete from Users where UserID=@userId", new { UserID = userId });
        }

        public Users[] InClause(IEnumerable<string> searchEmails)
        {
            var query = "select * from Users where Email in @Email";
            return connection.Query<Users>(query, new { Email = searchEmails }).ToArray();
        }

        public Tuple<List<Products>, List<Users>> MultipleReader()
        {
            var query = "select * from Product; select * from Users";
            var multipleReader = connection.QueryMultiple(query);

            var productList = multipleReader.Read<Products>();
            var userList = multipleReader.Read<Users>();

            multipleReader.Dispose();

            return new Tuple<List<Products>, List<Users>>(productList.ToList(), userList.ToList());
        }

        public Products[] JoinQuery(DateTime createTime)
        {
            var query = "select p.ProductName, p.CreateTime, u.UserName" +
                        " from Product as p" +
                        " join Users as u on p.UserID = u.UserID" +
                        " where p.CreateTime > @CreateTime";

            var result = connection.Query<Products, Users, Products>(query, (product, user) =>
            {
                product.UserOwner = user;
                return product;
            }, new { CreateTime = createTime }, splitOn: "UserName");

            return result.ToArray();
        }

        public Users[] StoreProcedureQuery(int userId)
        {
            var result = connection.Query<Users>("sp_GetUsers", new {id = userId},
                commandType: CommandType.StoredProcedure);

            return result.ToArray();
        }
    }

    public class Products
    {
        public int ProductID { get; set; }
        public string ProductName { get; set; }
        public string ProductDesc { get; set; }
        public Users UserOwner { get; set; }
        public DateTime CreateTime { get; set; }

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            var product = obj as Products;

            return product.ProductName == this.ProductName && product.ProductDesc == this.ProductDesc;
        }

        public override int GetHashCode()
        {
            return this.ProductName.GetHashCode() ^ this.ProductDesc.GetHashCode();
        }
    }

    public class Users
    {
        public int UserID { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            var user = obj as Users;

            return user.UserName == this.UserName && user.Email == this.Email && user.Address == this.Address;
        }

        public override int GetHashCode()
        {
            return this.UserName.GetHashCode() ^ this.Email.GetHashCode() ^ this.Address.GetHashCode();
        }
    }
}
