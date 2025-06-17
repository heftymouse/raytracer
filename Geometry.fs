module Raytracer.Geometry

open System.Numerics
open Raytracer.Data
open Raytracer.Math

type Sphere =
    { Center: Vector3
      Radius: single
      Material: IMaterial }

    interface IHittable with
        member this.Material = this.Material

        member this.Hit(ray: Ray) =
            let (Ray(origin, direction)) = ray
            let oc = this.Center - origin
            let a = direction.LengthSquared()
            let h = Vector3.Dot(direction, oc)
            let c = oc.LengthSquared() - this.Radius ** 2f
            let d = h ** 2f - a * c

            if d < 0f then
                None
            else
                let ts = [ (h - sqrt d) / a; (h + sqrt d) / a ] |> List.filter (fun x -> x > 0.001f)

                match ts with
                | [] -> None
                | _ ->
                    let t = List.min ts
                    let p = Ray.at ray t
                    let n' = (p - this.Center) / this.Radius
                    let frontFace = Vector3.Dot(n', direction) <= 0f
                    let n = if frontFace then n' else -n'

                    Some
                        { Ray = ray
                          Point = p
                          Normal = n
                          T = t
                          FrontFace = frontFace
                          Material = this.Material }
