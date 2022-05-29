module ProductModel

open System.ComponentModel.DataAnnotations
open Microsoft.EntityFrameworkCore
open EntityFrameworkCore.FSharp.Extensions
open EntityFrameworkCore.FSharp.DbContextHelpers

[<CLIMutable>]
type Product = {
    [<Key>] Id: int
    Prefix: string
    Code: int
    Serial: int
    Postfix: string
    Name: string
}

type ProductsContext() =  
    inherit DbContext()
    
    [<DefaultValue>]
    val mutable products : DbSet<Product>

    member this.Product
        with get() = this.products
        and set v = this.products <- v

    override _.OnModelCreating builder =
        builder.RegisterOptionTypes() // enables option values for all entities

    override __.OnConfiguring(options: DbContextOptionsBuilder) : unit =
        options.UseSqlite("Data Source=F:\dev\programming\web\products_catalogue\products_catalogue.db") |> ignore
