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

type OutputOptions = { Width: int; Height: int; Spp: int }

type CameraOptions =
    { LookFrom: Vector3
      LookAt: Vector3
      VUp: Vector3
      Fov: float32
      FocusDistance: float32
      DefocusAngle: float32 }
