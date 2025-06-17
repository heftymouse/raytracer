open System.Numerics
open Raytracer.Data
open Raytracer.Math
open Raytracer.Material
open Raytracer.Geometry
open Raytracer.Output
open System
open System.IO

let rayColor (world: IHittable list) (spp: int) genSample center =
    [| for _ in 1..spp -> genSample center |]
    |> Array.map (fun ray ->
        let rec rayColorRec ray bounces =
            if bounces >= 10 then
                Vector3.Zero
            else
                let hits = world |> List.choose (fun h -> h.Hit ray)

                match hits with
                | [] ->
                    let (Ray(_, direction)) = ray
                    let unitDir = Vector3.Normalize direction
                    let a = 0.5f * unitDir.Y + 1f
                    Vector3.One * (1f - a) + Vector3(0.5f, 0.7f, 1f) * a
                | _ ->
                    let closest = hits |> List.minBy (fun hr -> hr.T)
                    let e = closest.Material.Scatter closest

                    match e with
                    | None -> Vector3.Zero
                    | Some(attenuation, newRay) -> attenuation * rayColorRec newRay (bounces + 1)

        rayColorRec ray 0)
    |> Array.sum
    |> fun x -> x / single spp

[<EntryPoint>]
let main _ =
    // output options
    let outWidth = 1280
    let outHeight = 720
    let spp = 32

    // camera options
    let lookFrom = Vector3(13f, 2f, 3f)
    let lookAt = Vector3.Zero
    let vUp = Vector3(0f, 1f, 0f)
    let fov = 20f |> Single.DegreesToRadians
    let focusDistance = 10f
    let defocusAngle = 0.6f |> Single.DegreesToRadians

    // scene
    let world: IHittable list =
        [ { Center = Vector3(0f, -1000f, 0f)
            Radius = 1000f
            Material = Diffuse(Vector3(0.5f, 0.5f, 0.5f)) }
          { Center = Vector3.UnitY
            Radius = 1f
            Material = Dielectric(1.5f) }
          { Center = Vector3(-4f, 1f, 0f)
            Radius = 1f
            Material = Diffuse(Vector3(0.4f, 0.2f, 0.1f)) }
          { Center = Vector3(4f, 1f, 0f)
            Radius = 1f
            Material = Metallic(Vector3(0.7f, 0.6f, 0.5f), 0f) } ]
        @ [ for a in -11 .. 10 do
                for b in -11 .. 10 ->
                    let matChoice = Random.Shared.NextSingle()

                    let material: IMaterial =
                        match matChoice with
                        | x when x > 0.95f -> Dielectric(1.5f)
                        | x when x > 0.9f -> Metallic(Vector3.random 0f 1f, Random.Shared.NextSingle())
                        | _ -> Diffuse(Vector3.random 0f 1f)

                    { Center =
                        Vector3(
                            single a + 0.9f * Random.Shared.NextSingle(),
                            0.2f,
                            single b + 0.9f * Random.Shared.NextSingle()
                        )
                      Radius = 0.2f
                      Material = material } ]

    // don't mess with below
    let imgHeight = 2f * tan (fov / 2f) * focusDistance
    let imgWidth = imgHeight / single outHeight * single outWidth

    let w = Vector3.Normalize(lookFrom - lookAt)
    let u = Vector3.Normalize(Vector3.Cross(vUp, w))
    let v = Vector3.Normalize(Vector3.Cross(w, u))

    let du = imgWidth / single outWidth
    let dv = imgHeight / single outHeight

    printfn "%A %A %A %A" imgWidth imgHeight du dv

    let defocusRadius = focusDistance * tan (defocusAngle / 2f)

    let defocusU = u * defocusRadius
    let defocusV = v * defocusRadius

    let genSample vec =
        let rayOrigin =
            if defocusAngle > 0f then
                let rec genVector () =
                    let v = Vector3.randomXY -1f 1f

                    if v.LengthSquared() > 1e-160f && v.LengthSquared() <= 1f then
                        v
                    else
                        genVector ()

                let defocusVector = genVector ()
                lookFrom + defocusU * defocusVector.X + defocusV * defocusVector.Y
            else
                lookFrom

        let rayDirection =
            vec + lookFrom - rayOrigin
            + u * du * Random.Shared.NextSingle()
            + v * dv * Random.Shared.NextSingle()
            |> Vector3.Normalize

        let e = Ray(rayOrigin, rayDirection)

        e

    let rays =
        [| for y in 0 .. (outHeight - 1) ->
               [| for x in 0 .. (outWidth - 1) ->
                      let x' = (single x / single outWidth - 0.5f) * imgWidth
                      let y' = -(single y / single outHeight - 0.5f) * imgHeight

                      u * x' + v * y' - w * focusDistance |] |]

    let rayColors =
        rays
        |> Array.Parallel.map (fun row -> row |> Array.map (rayColor world spp genSample))

    File.WriteAllText("out.ppm", toPPM outWidth outHeight rayColors)
    0
