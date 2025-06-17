module Raytracer.Output

open Raytracer.Math
open System.Numerics

let toPPM width height pixels =
    let inline toPPMValue (x: single) =
        (if x > 0f then sqrt x else 0f) |> clamp (0f, 1f) |> (*) 255f |> int

    let actualPixels =
        pixels
        |> Seq.map (Seq.map (fun (v: Vector3) -> toPPMValue v.X, toPPMValue v.Y, toPPMValue v.Z))

    let pixelLines =
        actualPixels
        |> Seq.map (fun row -> row |> Seq.map (fun (r, g, b) -> sprintf "%d %d %d" r g b) |> String.concat " ")
        |> String.concat "\n"

    sprintf
        "\
P3
%A %A
255
%s\
    "
        width
        height
        pixelLines
