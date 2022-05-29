open Saturn
open Giraffe
open Giraffe.ViewEngine
open ProductModel
open System.Linq
open Microsoft.AspNetCore.Http



let indexView (products : IQueryable<Product>) =
    html [] [
        head [] [
            meta [_charset "utf-8"]
            title [] [ str "Каталог изделий" ]
            link [
                _rel "stylesheet"
                _href "https://cdn.jsdelivr.net/npm/bootstrap@5.0.2/dist/css/bootstrap.min.css"
                ]
        ]
        body [] [
            div [ _class "bg-light w-50 p-3 h-100 d-inline-block" ] [
                div [ _class "row" ] [
                    div [ _class "page-hero d-flex align-items-center justify-content-center" ] [
                        h1 [] [ str "Каталог изделий" ]
                        ]
                    ]

                div [ _class "row" ] [
                    div [ _class "page-hero d-flex align-items-center justify-content-center" ] [
                        form [
                            _method "get"
                        ] [
                            input [
                                    _type "text"
                                    _id "code"
                                    _name "code"
                                    _placeholder "Код..."
                                    _maxlength "6"
                            ]
                            input [
                                _type "text"
                                _id "name"
                                _name "name"
                                _placeholder "Название..."
                                _maxlength "50"
                            ]

                            input [
                                _type "submit"
                                _name "submit"
                                _value "Найти"
                            ]
                            input [
                                _type "submit"
                                _name "submit"
                                _value "Добавить"
                            ]
                        ]
                        ]
                        
                    ]

                div [ _class "row" ] [
                    div [ _class "page-hero d-flex align-items-center justify-content-center" ] [
                        table [ _class "table"; _id "productsList" ] [
                            thead [] [
                                tr [] [
                                    th [ _scope "col" ] [ str "Код организации" ]
                                    th [ _scope "col" ] [ str "Код классификатора" ]
                                    th [ _scope "col" ] [ str "Номер" ]
                                    th [ _scope "col" ] [ str "Наименование" ]
                                ]
                            ]
                            yield!
                                products
                                |> Seq.map (fun b -> tr [ _onclick "copyDecimalToClipboard(this)" ] [
                                        td [] [ str b.Prefix ]
                                        td [] [ str $"%06i{b.Code}" ]
                                        td [] [ str $"%03i{b.Serial}" ]
                                        td [] [ str b.Name ]
                                    ]
                                )
                        ]
                    ]
                ]
            ]
            script [
                _src "script.js"
                _type "text/javascript"
            ] []
        ]
    ]


let searchByCode (codeInput: int) =
    let ctx = new ProductModel.ProductsContext()
    query {
        for product in ctx.Product do
        where (product.Code = codeInput)
        sortByDescending product.Code
        sortByDescending product.Serial
    }


let searchByName (name: string) =
    let ctx = new ProductModel.ProductsContext()
    query {
        for product in ctx.Product do
        where (product.Name = name)
        sortByDescending product.Code
        sortByDescending product.Serial
    }


let baseList =
    let ctx = new ProductModel.ProductsContext()
    query {
        for product in ctx.Product do
        sortByDescending product.Code
        sortByDescending product.Serial
    }


let addNewProduct (code: int) (name: string ) =
    let ctx = new ProductModel.ProductsContext()
    let product =
        query {
            for product in ctx.Product do
            where (product.Code = code)
            sortByDescending product.Serial
            head
    }

    let newRecord = {
        product with Id = 0; Serial = product.Serial + 1; Name = name
    }
    
    ctx.Add(newRecord) |> ignore
    ctx.SaveChanges() |> ignore

    searchByCode code



[<CLIMutable>]
type SearchInput =
    {
    code : string
    name : string
    submit : string
    }


let tryToInt (s: string) = 
    match System.Int32.TryParse s with
    | true, v -> Some v
    | false, _ -> None



let doSearch (input: SearchInput) =
    let code =
        if isNull input.code then
            None
        else
            tryToInt input.code

    let name =
        if isNull input.name then
            None
        else
            match input.name with
            | "" -> None
            | _ -> Some input.name

    match code with
    | Some v -> searchByCode v
    | None ->
        match name with
        | Some v -> searchByName v
        | None -> baseList


let doAdd (input: SearchInput) =
    let code =
        if isNull input.code then
            None
        else
            tryToInt input.code

    let name =
        if isNull input.name then
            ""
        else
            input.name

    match code with
    | Some v -> addNewProduct v name
    | None -> baseList


let handleInput (input: SearchInput) =
    match input.submit with
    | "Найти" -> doSearch input
    | "Добавить" -> doAdd input
    | _ -> baseList


let getIndexList (codeInput: string) =
    let ctx = new ProductModel.ProductsContext()

    match codeInput with
    | "" -> query {
            for product in ctx.Product do
            sortByDescending product.Code
            sortByDescending product.Serial
        }
    | _ -> 
        let code = codeInput |> int
        query {
            for product in ctx.Product do
            where (product.Code = code)
            sortByDescending product.Code
            sortByDescending product.Serial
        }



let someHttpHandler : HttpHandler =
    fun (next : HttpFunc) (ctx : HttpContext) ->
        let searchInput = ctx.BindQueryString<SearchInput>()
        let document = indexView (handleInput searchInput)
        let bytes = RenderView.AsBytes.htmlDocument document
        ctx.SetContentType "text/html; charset=utf-8"
        ctx.WriteBytesAsync bytes


let defaultView = router {
    get "/" someHttpHandler
}

let browserRouter = router {
    forward "" defaultView // Use the default view
}

let appRouter = router {
    forward "" browserRouter
}

let app = application {
    use_router appRouter
    use_static "static"
}

run app