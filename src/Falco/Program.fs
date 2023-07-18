open System
open System.IO
open System.Threading.Tasks

open Falco
open Falco.Routing
open Falco.HostBuilder
open Falco.Markup
open Microsoft.AspNetCore.Http

let responseTemplate color content =

    Elem.article
        [ Attr.style $"color: %s{color}" ]
        [ Elem.header [] [ Elem.h3 [] [ Text.enc "File Contents below:" ] ]
          Elem.pre [] [ Text.enc content ] ]

let index (context: HttpContext) : Task =
    task {
        let! content = File.ReadAllTextAsync("../index.html", context.RequestAborted)
        return! Response.ofHtmlString content context
    }


let handler context : Task =
    task {
        // Falco can also use aspnet's features directly
        // but offers an F# API for ease of use
        let! form = Request.streamForm context

        let extractedFile =
            // extract the file safely from the
            // IFormFileCollection in the http context
            form.Files
            |> Option.bind (fun form ->
                // try to extract the uploaded file named after the "name" attribute in html
                // GetFile returns null if no file is present, so we safely convert it into an optional value
                form.GetFile "my-uploaded-file" |> Option.ofObj)

        match extractedFile with
        | Some file ->
            use reader = new StreamReader(file.OpenReadStream())

            // read the file stream directly into text for simplicity
            let! content = reader.ReadToEndAsync(context.RequestAborted)

            // you could do this with the streams themselves to make it more performat
            do! File.WriteAllTextAsync($"./{Guid.NewGuid()}.txt", content, context.RequestAborted)

            // return a response for the request
            let content = responseTemplate "green" content
            return! Response.ofHtml content context
        | None ->
            // The file was not found in the request return something
            let content = responseTemplate "tomato" "The file was not provided"

            return! context |> Response.withStatusCode 400 |> Response.ofHtml content
    }


// declare the host and the endpoints
webHost [||] {
    endpoints
        [
          // handle the index routes as well
          get "/" index
          get "/index.html" index
          // our file endpoint handler as well
          post "/uploads" handler ]
}
