open System
open System.Numerics
open System.IO
open Raytracer.Data
open Raytracer.Math
open Raytracer.Geometry
open Raytracer.Material
open Raytracer.Output
open Raytracer.Main

[<EntryPoint>]
let main _ =
    // output options
    let output =
        { Width = 640
          Height = 480
          Spp = 32 }

    // camera options
    let camera =
        { LookFrom = Vector3(13f, 2f, 3f)
          LookAt = Vector3.Zero
          VUp = Vector3(0f, 1f, 0f)
          Fov = 20f |> Single.DegreesToRadians
          FocusDistance = 10f
          DefocusAngle = 0.6f |> Single.DegreesToRadians }

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

    renderScene output camera world
    |> toPPM output.Width output.Height
    |> fun e -> File.WriteAllText("out.ppm", e)

    0
