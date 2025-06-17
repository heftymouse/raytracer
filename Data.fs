module rec Raytracer.Data

open System.Numerics
open Raytracer.Math

type HitRecord =
    { Ray: Ray
      T: single
      Point: Vector3
      Normal: Vector3
      FrontFace: bool
      Material: IMaterial }

type ScatterRecord = (Vector3 * Ray) option

type IMaterial =
    abstract member Scatter: HitRecord -> ScatterRecord

type IHittable =
    abstract member Material: IMaterial
    abstract member Hit: Ray -> HitRecord option
