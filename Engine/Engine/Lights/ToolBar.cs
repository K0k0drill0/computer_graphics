using Engine.Camera;
using Engine.Shaders;
using Engine.ToolBar;

using OpenTK.Mathematics;

namespace Engine.Lights
{
    internal class ToolBar
    {
        public static void AddLight(Shader shader, Light light, BaseCamera camera)
        {
            shader.Use();

            if (light == null) return;


            if (light.Count(LightType.PointLight) != 0)
            {
                shader.SetInt("nrPL", light.Count(LightType.PointLight));

                for (int i = 0; i < light.Count(LightType.PointLight); i++)
                {
                    shader.SetVector3($"pointLights[{i}].position", light.GetPosition(LightType.PointLight, i));
                    shader.SetVector3($"pointLights[{i}].color",    light.GetColor(LightType.PointLight, i));

                    shader.SetFloat($"pointLights[{i}].constant",   light.GetArguments(LightType.PointLight, i).X);
                    shader.SetFloat($"pointLights[{i}].linear",     light.GetArguments(LightType.PointLight, i).Y);
                    shader.SetFloat($"pointLights[{i}].quadratic",  light.GetArguments(LightType.PointLight, i).Z);
                }
            }
            else shader.SetInt("nrPL", 0);

            if (light.Count(LightType.SpotLight) != 0)
            {
                shader.SetInt("nrSL", light.Count(LightType.SpotLight));

                for (int i = 0; i < light.Count(LightType.SpotLight); i++)
                {
                    shader.SetVector3($"spotLights[{i}].position",  light.GetPosition(LightType.SpotLight, i));
                    shader.SetVector3($"spotLights[{i}].direction", light.GetDirection(LightType.SpotLight, i));
                    shader.SetVector3($"spotLights[{i}].color",     light.GetColor(LightType.SpotLight, i));

                    shader.SetFloat($"spotLights[{i}].cutOff",      MathF.Cos(MathHelper.DegreesToRadians(light.GetCutOff(LightType.SpotLight, i).X)));
                    shader.SetFloat($"spotLights[{i}].outerCutOff", MathF.Cos(MathHelper.DegreesToRadians(light.GetCutOff(LightType.SpotLight, i).Y)));
                    
                    shader.SetFloat($"spotLights[{i}].constant",  light.GetArguments(LightType.SpotLight, i).X);
                    shader.SetFloat($"spotLights[{i}].linear",    light.GetArguments(LightType.SpotLight, i).Y);
                    shader.SetFloat($"spotLights[{i}].quadratic", light.GetArguments(LightType.SpotLight, i).Z);
                }
            }
            else shader.SetInt("nrSL", 0);

            if (light.Count(LightType.DirectionLight) != 0)
            {
                shader.SetInt("nrDL", light.Count(LightType.DirectionLight));

                for (int i = 0; i < light.Count(LightType.DirectionLight); i++)
                {
                    shader.SetMatrix4($"directionLights[{i}].view",       new DirectionLightCamera(camera, light.GetDirection(LightType.DirectionLight, 0)).GetViewMatrix());
                    shader.SetMatrix4($"directionLights[{i}].projection", new DirectionLightCamera(camera, light.GetDirection(LightType.DirectionLight, 0)).GetProjectionMatrix());
                    
                    shader.SetVector3($"directionLights[{i}].direction", light.GetDirection(LightType.DirectionLight, i));
                    shader.SetVector3($"directionLights[{i}].color",     light.GetColor(LightType.DirectionLight, i));
                    shader.SetInt($"directionLights[{i}].shadowMap",     light.GetShadows(LightType.DirectionLight, 0).Use());
                }
            }
            else shader.SetInt("nrDL", 0);
        }

    }
}
