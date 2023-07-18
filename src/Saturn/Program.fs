open Saturn
open Giraffe

let handler name next context =
    let message = $"Hello %s{name}"
    // return an http response
    text message next context

let appRouter =
    router {
        // bind route parameters safely
        get "/" (handler "F#")
        getf "/%s" handler
    }

let server = application { use_router appRouter }

run server
