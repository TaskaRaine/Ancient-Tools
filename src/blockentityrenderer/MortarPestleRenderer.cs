using System;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;

namespace AncientTools.BlockEntityRenderer
{
    class MortarPestleRenderer : IRenderer
    {
        public double RenderOrder { get { return 0.5; } }

        public int RenderRange => 24;

        private ICoreClientAPI api;
        private BlockPos pos;


        MeshRef meshref;
        public Matrixf ModelMat = new Matrixf();

        public bool ShouldRender = false;
        public bool ShouldAnimate = false;

        private Vec3f lookAtPlayerVector = new Vec3f(0.0f, 0.0f, 0.0f);
        private float[] animationPositions = { 0.025f, 0.025f, 0.0375f, 0.05f, 0.0625f, 0.075f, 0.0625f, 0.05f, 0.0375f, 0.025f, 0.025f };
        private Vec3f[] animationRotations = {
            new Vec3f(0.0f, 0.15f, 0.0f),
            new Vec3f(0.0f, 0.10f, 0.0f),
            new Vec3f(0.0f, 0.05f, 0.0f),
            new Vec3f(0f, 0.025f, 0f),
            new Vec3f(0f, 0.0f, 0f),
            new Vec3f(0f, 0.0f, 0f),
            new Vec3f(0f, 0.0f, 0f),
            new Vec3f(0f, 0.025f, 0f),
            new Vec3f(0.0f, 0.05f, 0.0f),
            new Vec3f(0.0f, 0.10f, 0.0f),
            new Vec3f(0.0f, 0.15f, 0.0f)
        };

        private int animPosition = 0;

        public MortarPestleRenderer(ICoreClientAPI coreClientAPI, BlockPos pos)
        {
            this.api = coreClientAPI;
            this.pos = pos;
        }

        public void UpdateMesh(MeshData mesh)
        {
            if(mesh != null)
            {
                if (meshref == null)
                    meshref = api.Render.UploadMesh(mesh);
                else
                    api.Render.UpdateMesh(meshref, mesh);
            }
        }
        public void Dispose()
        {
            api.Event.UnregisterRenderer(this, EnumRenderStage.Opaque);

            if(meshref != null)
                meshref.Dispose();
        }

        public void OnRenderFrame(float deltaTime, EnumRenderStage stage)
        {
            if (meshref == null || !ShouldRender) return;

            IRenderAPI rpi = api.Render;
            Vec3d camPos = api.World.Player.Entity.CameraPos;

            rpi.GlDisableCullFace();
            rpi.GlToggleBlend(true);

            IStandardShaderProgram prog = rpi.PreparedStandardShader(pos.X, pos.Y, pos.Z);
            prog.Tex2D = api.BlockTextureAtlas.AtlasTextureIds[0];


            prog.ModelMatrix = ModelMat
                .Identity()
                .Translate(pos.X - camPos.X, pos.Y - camPos.Y, pos.Z - camPos.Z)
                .Translate(0.5f, 0, 0.5f)
                .RotateX(lookAtPlayerVector.X + animationRotations[animPosition].X)
                .RotateY(lookAtPlayerVector.Y + animationRotations[animPosition].Y)
                .RotateZ(lookAtPlayerVector.Z + animationRotations[animPosition].Z)
                .Translate(-0.5f, 0, -0.5f)
                .Translate(0, animationPositions[animPosition], 0)
                .Values;

            prog.ViewMatrix = rpi.CameraMatrixOriginf;
            prog.ProjectionMatrix = rpi.CurrentProjectionMatrix;
            rpi.RenderMesh(meshref);
            prog.Stop();

            if(ShouldAnimate)
                SetPestleAnimationFrame();
        }
        //-- Sets the look at vector in such a way that the pestle appears to be held in hand anytime the mortar is being used --//
        public void SetPestleLookAtVector(float x, float y, float z)
        {
            lookAtPlayerVector.X = x;
            lookAtPlayerVector.Y = y;
            lookAtPlayerVector.Z = z;
        }
        public void SetPestleLookAtVector(IPlayer byPlayer)
        {
            Vec3f normal = ((pos.ToVec3f() + new Vec3f(0.5f, 0, 0.5f)) - byPlayer.Entity.Pos.XYZFloat);
            normal.Y = 0.325f;

            double pitch = Math.Asin(normal.Y);
            double yaw = Math.Atan2(normal.X, normal.Z);

            lookAtPlayerVector.Y = (float)yaw;
            lookAtPlayerVector.Z = (float)pitch;
        }
        public Vec3f GetPestleLookAtVector()
        {
            return lookAtPlayerVector;
        }
        public Vec3f GetInitialAnimationRotation()
        {
            return new Vec3f(animationRotations[0].X, animationRotations[0].Y, animationRotations[0].Z);
        }
        public void ResetAnimation()
        {
            animPosition = 0;
        }
        //-- Tick to the next animation frame any time the pestle is being used --//
        private void SetPestleAnimationFrame()
        {
            if (animPosition < animationPositions.Length - 1)
                animPosition++;
            else
                animPosition = 0;
        }
    }
}
