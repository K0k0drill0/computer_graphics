using Engine.Camera;
using Engine.Objects.Objects3D;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace Engine.Lights
{
    public enum LightType
    {
        PointLight,
        DirectionLight,
        SpotLight
    };

    public enum Model
    {
        Direction,
        Color,
    };

    internal class Light
    {
        public readonly ObjectManager objectManager;

        public Light(ObjectManager objectManager)
        {
            this.objectManager = objectManager;
        }

        public PointLight PointLight { get; private set; }
        public SpotLight SpotLight { get; private set; }
        public DirectionLight DirectionLight { get; private set; }

        public void Draw(BaseCamera camera, Light light, bool isPhysicsWorld)
        {
            //PointLight?.Draw(camera, light, isPhysicsWorld);
            //SpotLight?.Draw(camera, light, isPhysicsWorld);
        }
        public void CreateSpotLight()
        {
            SpotLight ??= new SpotLight(objectManager);
            SpotLight.Add(SpotLight.Name);
        }
        public void CreateSpotLight(Vector3 color, Vector3 arguments, Vector2 edges, Vector3 direction, Object obj, int idx)
        {
            SpotLight ??= new SpotLight(objectManager);
            SpotLight.Create(color, arguments, edges, direction, obj, idx);
        }

        public void CreatePointLight() 
        {
            PointLight ??= new PointLight(objectManager);
            PointLight.Add(PointLight.Name); 
        }
        public void CreatePointLight(Vector3 color, Vector3 arguments, Object obj, int idx)
        {
            PointLight ??= new PointLight(objectManager);
            PointLight.Create(color, arguments, obj, idx);
        }
        public void CreateDirectionLight()
        {
            DirectionLight ??= new DirectionLight(objectManager);
            DirectionLight.Create();
        }
        public void CreateDirectionLight(Vector3 color, Vector3 direction)
        {
            DirectionLight ??= new DirectionLight(objectManager);
            DirectionLight.Create(color, direction);
        }

        public int Count(LightType type)
        {
            return type switch
            {
                LightType.SpotLight => SpotLight?.Count ?? 0,
                LightType.PointLight => PointLight?.Count ?? 0,
                LightType.DirectionLight => DirectionLight?.Count ?? 0,
                _ => throw new Exception("Check type"),
            };
        }

        public Vector3 GetPosition(LightType type, int num)
        {
            return type switch
            {
                LightType.SpotLight => SpotLight.GetPosition(num),
                LightType.PointLight => PointLight.GetPosition(num),
                _ => throw new Exception("Check type"),
            };
        }

        public Vector3 GetDirection(LightType type, int num)
        {
            return type switch
            {
                LightType.SpotLight => SpotLight.GetDirection(num),
                LightType.DirectionLight => DirectionLight.GetDirection(num),
                _ => throw new Exception("Check type"),
            };
        }

        public Vector3 GetColor(LightType type, int num)
        {
            return type switch
            {
                LightType.SpotLight => SpotLight.GetColor(num),
                LightType.DirectionLight => DirectionLight.GetColor(num),
                LightType.PointLight => PointLight.GetColor(num),
                _ => throw new Exception("Check type"),
            };
        }

        public Vector3 GetArguments(LightType type, int num)
        {
            return type switch
            {
                LightType.SpotLight => SpotLight.GetArguments(num),
                LightType.PointLight => PointLight.GetArguments(num),
                _ => throw new Exception("Check type"),
            };
        }

        public Vector2 GetCutOff(LightType type, int num)
        {
            return type switch
            {
                LightType.SpotLight => SpotLight.GetCutOff(num),
                _ => throw new Exception("Check type"),
            };
        }

        public Shadow GetShadows(LightType type, int num)
        {
            return type switch
            {
                LightType.DirectionLight => DirectionLight.GetShadow(num),
                _ => throw new Exception("Check type"),
            };
        }

        public static void Load(Light light, LightType type, ref string[] settings)
        {
            switch (type)
            {
                case LightType.PointLight:
                    PointLight.Load(light, ref settings);
                    break;
                //case LightType.SpotLight:
                //    SpotLight.Load(light, ref settings);
                //    break;
                case LightType.DirectionLight:
                    DirectionLight.Load(light, ref settings);
                    break;

                default:
                    throw new Exception("Check type");
            };
        }
    }
}
