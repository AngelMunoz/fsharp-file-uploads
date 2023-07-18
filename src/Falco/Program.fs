open Falco
open Falco.Routing
open Falco.HostBuilder

let handler context =
    let route = Request.getRoute context

    let name = // extract route values safely
        match route.TryGetStringNonEmpty "Name" with
        | Some name -> name
        | None -> "F#"

    // send back an http response
    Response.ofPlainText $"Hello {name}" context

// declare the host and the endpoints
webHost [||] { endpoints [ get "/{Name?}" handler ] }
