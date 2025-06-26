module Raytracer.Main

open System.Numerics
open Raytracer.Data
open Raytracer.Math
open System

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
                    let a = 0.5f * (unitDir.Y + 1f)
                    Vector3.One * (1f - a) + Vector3(0.5f, 0.7f, 1f) * a
                | _ ->
                    let closest = hits |> List.minBy (fun hr -> hr.T)
                    let e = closest.Material.Scatter closest

                    match e with
                    | None -> Vector3.Zero
                    | Some(newAttenuation, newRay) -> newAttenuation * rayColorRec newRay (bounces + 1)

        rayColorRec ray 0)
    |> Array.sum
    |> fun x -> x / single spp

let renderScene (output: OutputOptions) (camera: CameraOptions) (world: IHittable list) =
    let imgHeight = 2f * tan (camera.Fov / 2f) * camera.FocusDistance
    let imgWidth = imgHeight / single output.Height * single output.Width

    let w = Vector3.Normalize(camera.LookFrom - camera.LookAt)
    let u = Vector3.Normalize(Vector3.Cross(camera.VUp, w))
    let v = Vector3.Normalize(Vector3.Cross(w, u))

    let du = imgWidth / single output.Width
    let dv = imgHeight / single output.Height

    let defocusRadius = camera.FocusDistance * tan (camera.DefocusAngle / 2f)

    let defocusU = u * defocusRadius
    let defocusV = v * defocusRadius

    let genSample vec =
        let rayOrigin =
            if camera.DefocusAngle > 0f then
                let rec genVector () =
                    let v = Vector3.randomXY -1f 1f

                    if v.LengthSquared() > 1e-160f && v.LengthSquared() <= 1f then
                        v
                    else
                        genVector ()

                let defocusVector = genVector ()
                camera.LookFrom + defocusU * defocusVector.X + defocusV * defocusVector.Y
            else
                camera.LookFrom

        let rayDirection =
            vec + camera.LookFrom - rayOrigin
            + u * du * Random.Shared.NextSingle()
            + v * dv * Random.Shared.NextSingle()
            |> Vector3.Normalize

        let e = Ray(rayOrigin, rayDirection)
        e

    let rays =
        [| for y in 0 .. (output.Height - 1) do
               for x in 0 .. (output.Width - 1) ->
                   let x' = (single x / single output.Width - 0.5f) * imgWidth
                   let y' = -(single y / single output.Height - 0.5f) * imgHeight

                   u * x' + v * y' - w * camera.FocusDistance |]

    let rows = [| for y in 0 .. (output.Height - 1) -> y * output.Width |]

    let rayColors =
        rows
        |> Array.Parallel.map (fun row ->
            let thing = Array.zeroCreate output.Width

            for i in 0 .. output.Width - 1 do
                thing[i] <- rayColor world output.Spp genSample rays[row + i]

            thing)

    rayColors
