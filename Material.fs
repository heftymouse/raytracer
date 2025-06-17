module rec Raytracer.Material

open System.Numerics
open Raytracer.Math
open Raytracer.Data
open System

type Diffuse(albedo: Vector3) =
    interface IMaterial with
        member _.Scatter(hit: HitRecord) : ScatterRecord =
            let rec genVector () =
                let v = Vector3.random -1f 1f

                if v.LengthSquared() > 1e-160f && v.LengthSquared() <= 1f then
                    v
                else
                    genVector ()

            let v1 = genVector ()
            let v = hit.Normal + v1

            Some(albedo, Ray(hit.Point, v))

type Normal() =
    interface IMaterial with
        member _.Scatter(hit: HitRecord) : ScatterRecord =
            Some((hit.Normal + Vector3.One) * 0.5f, Ray(hit.Point, hit.Normal))

type Metallic(albedo: Vector3, fuzz: single) =
    interface IMaterial with
        member _.Scatter(hit: HitRecord) : ScatterRecord =
            let (Ray(_, direction)) = hit.Ray
            Some(albedo, Ray(hit.Point, Vector3.Reflect(direction, hit.Normal) + Vector3.randomUnit * fuzz))

type Dielectric(refractiveIndex: single) =
    interface IMaterial with
        member _.Scatter(hit: HitRecord) : ScatterRecord =
            let ri =
                if hit.FrontFace then
                    1f / refractiveIndex
                else
                    refractiveIndex

            let (Ray(_, direction)) = hit.Ray
            let unitDirection = Vector3.Normalize direction
            let cosTheta = min (Vector3.Dot(-unitDirection, hit.Normal)) 1f
            let sinTheta = sqrt (1f - cosTheta ** 2f)

            let schlickReflectance =
                let r0 = ((1f - ri) / (1f + ri)) ** 2f
                r0 + (1f - r0) * (1f - cosTheta) ** 5f

            let outRay =
                if ri * sinTheta > 1f || schlickReflectance > Random.Shared.NextSingle() then
                    Vector3.Reflect(unitDirection, hit.Normal)
                else
                    let rPerp = (unitDirection + hit.Normal * cosTheta) * ri
                    let rParallel = hit.Normal * -(sqrt (abs 1f - rPerp.LengthSquared()))
                    rPerp + rParallel

            Some(Vector3(1f, 1f, 1f), Ray(hit.Point, outRay))
