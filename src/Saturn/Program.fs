open System
open System.IO

open Microsoft.AspNetCore.Http

open Saturn
open Giraffe
open Giraffe.ViewEngine

let responseTemplate color content =

    article
        [ _style $"color: %s{color}" ]
        [ header [] [ h3 [] [ encodedText "File Contents below:" ] ]
          pre [] [ encodedText content ] ]

let index next context = htmlFile "../index.html" next context

let handler next (context: HttpContext) =
    task {
        // Saturn/Giraffe can also use aspnet's features directly
        let form = context.Request.Form
        let extractedFile = form.Files.GetFile "my-uploaded-file" |> Option.ofObj

        match extractedFile with
        | Some file ->
            use reader = new StreamReader(file.OpenReadStream())

            // read the file stream directly into text for simplicity
            let! content = reader.ReadToEndAsync()

            // you could do this with the streams themselves to make it more performat
            do! File.WriteAllTextAsync($"./{Guid.NewGuid()}.txt", content)

            // return a response for the request
            let content = responseTemplate "green" content
            return! htmlView content next context
        | None ->
            // The file was not found in the request return something
            let content = responseTemplate "tomato" "The file was not provided"

            return! (setStatusCode 400 >> htmlView content) next context
    }

let appRouter =
    router {
        // bind route parameters safely
        get "/" index
        get "/index.html" index
        post "/uploads" handler
    }

let server = application { use_router appRouter }

run server
