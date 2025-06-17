module rec Raytracer.Math

open System
open System.Numerics

type Vector3 with
    static member random min max =
        let r () =
            Random.Shared.NextSingle() * (max - min) + min

        Vector3(r (), r (), r ())

    static member randomXY min max =
        let r () =
            Random.Shared.NextSingle() * (max - min) + min

        Vector3(r (), r (), 0f)

    static member randomUnit = Vector3.random 0f 1f |> Vector3.Normalize

type Ray = Ray of Vector3 * Vector3 with
    static member at (Ray(origin, direction)) (t: single): Vector3 = origin + direction * t
    
let clamp (min, max) value =
    if value < min then min
    elif value > max then max
    else value
