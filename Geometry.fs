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
            let c = oc.LengthSquared() - this.Radius * this.Radius
            let d = h * h - a * c

            if d < 0f then
                None
            else
                let t =
                    let t' = (h - sqrt d) / a

                    if t' < 0.001f then
                        let t' = (h + sqrt d) / a
                        if t' < 0.001f then None else Some t'
                    else
                        Some t'

                match t with
                | None -> None
                | Some t ->
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
