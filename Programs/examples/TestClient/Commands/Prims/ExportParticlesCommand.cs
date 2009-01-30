using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using OpenMetaverse;

namespace OpenMetaverse.TestClient
{
    public class ExportParticlesCommand : Command
    {
        public ExportParticlesCommand(TestClient testClient)
        {
            Name = "exportparticles";
            Description = "Reverse engineers a prim with a particle system to an LSL script. Usage: exportscript [prim-Guid]";
            Category = CommandCategory.Objects;
        }

        public override string Execute(string[] args, Guid fromAgentID)
        {
            if (args.Length != 1)
                return "Usage: exportparticles [prim-Guid]";

            Guid id;
            if (!GuidExtensions.TryParse(args[0], out id))
                return "Usage: exportparticles [prim-Guid]";

            lock (Client.Network.Simulators)
            {
                for (int i = 0; i < Client.Network.Simulators.Count; i++)
                {
                    Primitive exportPrim = Client.Network.Simulators[i].ObjectsPrimitives.Find(
                        delegate(Primitive prim)
                        {
                            return prim.ID == id;
                        }
                    );

                    if (exportPrim != null)
                    {
                        if (exportPrim.ParticleSys.CRC != 0)
                        {
                            StringBuilder lsl = new StringBuilder();

                            #region Particle System to LSL

                            lsl.Append("default" + Environment.NewLine);
                            lsl.Append("{" + Environment.NewLine);
                            lsl.Append("    state_entry()" + Environment.NewLine);
                            lsl.Append("    {" + Environment.NewLine);
                            lsl.Append("         llParticleSystem([" + Environment.NewLine);

                            lsl.Append("         PSYS_PART_FLAGS, 0");

                            if ((exportPrim.ParticleSys.PartDataFlags & Primitive.ParticleSystem.ParticleDataFlags.InterpColor) != 0)
                                lsl.Append(" | PSYS_PART_INTERP_COLOR_MASK");
                            if ((exportPrim.ParticleSys.PartDataFlags & Primitive.ParticleSystem.ParticleDataFlags.InterpScale) != 0)
                                lsl.Append(" | PSYS_PART_INTERP_SCALE_MASK");
                            if ((exportPrim.ParticleSys.PartDataFlags & Primitive.ParticleSystem.ParticleDataFlags.Bounce) != 0)
                                lsl.Append(" | PSYS_PART_BOUNCE_MASK");
                            if ((exportPrim.ParticleSys.PartDataFlags & Primitive.ParticleSystem.ParticleDataFlags.Wind) != 0)
                                lsl.Append(" | PSYS_PART_WIND_MASK");
                            if ((exportPrim.ParticleSys.PartDataFlags & Primitive.ParticleSystem.ParticleDataFlags.FollowSrc) != 0)
                                lsl.Append(" | PSYS_PART_FOLLOW_SRC_MASK");
                            if ((exportPrim.ParticleSys.PartDataFlags & Primitive.ParticleSystem.ParticleDataFlags.FollowVelocity) != 0)
                                lsl.Append(" | PSYS_PART_FOLLOW_VELOCITY_MASK");
                            if ((exportPrim.ParticleSys.PartDataFlags & Primitive.ParticleSystem.ParticleDataFlags.TargetPos) != 0)
                                lsl.Append(" | PSYS_PART_TARGET_POS_MASK");
                            if ((exportPrim.ParticleSys.PartDataFlags & Primitive.ParticleSystem.ParticleDataFlags.TargetLinear) != 0)
                                lsl.Append(" | PSYS_PART_TARGET_LINEAR_MASK");
                            if ((exportPrim.ParticleSys.PartDataFlags & Primitive.ParticleSystem.ParticleDataFlags.Emissive) != 0)
                                lsl.Append(" | PSYS_PART_EMISSIVE_MASK");

                            lsl.Append(","); lsl.Append(Environment.NewLine);
                            lsl.Append("         PSYS_SRC_PATTERN, 0");

                            if ((exportPrim.ParticleSys.Pattern & Primitive.ParticleSystem.SourcePattern.Drop) != 0)
                                lsl.Append(" | PSYS_SRC_PATTERN_DROP");
                            if ((exportPrim.ParticleSys.Pattern & Primitive.ParticleSystem.SourcePattern.Explode) != 0)
                                lsl.Append(" | PSYS_SRC_PATTERN_EXPLODE");
                            if ((exportPrim.ParticleSys.Pattern & Primitive.ParticleSystem.SourcePattern.Angle) != 0)
                                lsl.Append(" | PSYS_SRC_PATTERN_ANGLE");
                            if ((exportPrim.ParticleSys.Pattern & Primitive.ParticleSystem.SourcePattern.AngleCone) != 0)
                                lsl.Append(" | PSYS_SRC_PATTERN_ANGLE_CONE");
                            if ((exportPrim.ParticleSys.Pattern & Primitive.ParticleSystem.SourcePattern.AngleConeEmpty) != 0)
                                lsl.Append(" | PSYS_SRC_PATTERN_ANGLE_CONE_EMPTY");

                            lsl.Append("," + Environment.NewLine);

                            lsl.Append("         PSYS_PART_START_ALPHA, " + String.Format("{0:0.00000}", exportPrim.ParticleSys.PartStartColor.A) + "," + Environment.NewLine);
                            lsl.Append("         PSYS_PART_END_ALPHA, " + String.Format("{0:0.00000}", exportPrim.ParticleSys.PartEndColor.A) + "," + Environment.NewLine);
                            lsl.Append("         PSYS_PART_START_COLOR, " + exportPrim.ParticleSys.PartStartColor.ToRGBString() + "," + Environment.NewLine);
                            lsl.Append("         PSYS_PART_END_COLOR, " + exportPrim.ParticleSys.PartEndColor.ToRGBString() + "," + Environment.NewLine);
                            lsl.Append("         PSYS_PART_START_SCALE, <" + String.Format("{0:0.00000}", exportPrim.ParticleSys.PartStartScaleX) + ", " + String.Format("{0:0.00000}", exportPrim.ParticleSys.PartStartScaleY) + ", 0>, " + Environment.NewLine);
                            lsl.Append("         PSYS_PART_END_SCALE, <" + String.Format("{0:0.00000}", exportPrim.ParticleSys.PartEndScaleX) + ", " + String.Format("{0:0.00000}", exportPrim.ParticleSys.PartEndScaleY) + ", 0>, " + Environment.NewLine);
                            lsl.Append("         PSYS_PART_MAX_AGE, " + String.Format("{0:0.00000}", exportPrim.ParticleSys.PartMaxAge) + "," + Environment.NewLine);
                            lsl.Append("         PSYS_SRC_MAX_AGE, " + String.Format("{0:0.00000}", exportPrim.ParticleSys.MaxAge) + "," + Environment.NewLine);
                            lsl.Append("         PSYS_SRC_ACCEL, " + exportPrim.ParticleSys.PartAcceleration.ToString() + "," + Environment.NewLine);
                            lsl.Append("         PSYS_SRC_BURST_PART_COUNT, " + String.Format("{0:0}", exportPrim.ParticleSys.BurstPartCount) + "," + Environment.NewLine);
                            lsl.Append("         PSYS_SRC_BURST_RADIUS, " + String.Format("{0:0.00000}", exportPrim.ParticleSys.BurstRadius) + "," + Environment.NewLine);
                            lsl.Append("         PSYS_SRC_BURST_RATE, " + String.Format("{0:0.00000}", exportPrim.ParticleSys.BurstRate) + "," + Environment.NewLine);
                            lsl.Append("         PSYS_SRC_BURST_SPEED_MIN, " + String.Format("{0:0.00000}", exportPrim.ParticleSys.BurstSpeedMin) + "," + Environment.NewLine);
                            lsl.Append("         PSYS_SRC_BURST_SPEED_MAX, " + String.Format("{0:0.00000}", exportPrim.ParticleSys.BurstSpeedMax) + "," + Environment.NewLine);
                            lsl.Append("         PSYS_SRC_INNERANGLE, " + String.Format("{0:0.00000}", exportPrim.ParticleSys.InnerAngle) + "," + Environment.NewLine);
                            lsl.Append("         PSYS_SRC_OUTERANGLE, " + String.Format("{0:0.00000}", exportPrim.ParticleSys.OuterAngle) + "," + Environment.NewLine);
                            lsl.Append("         PSYS_SRC_OMEGA, " + exportPrim.ParticleSys.AngularVelocity.ToString() + "," + Environment.NewLine);
                            lsl.Append("         PSYS_SRC_TEXTURE, (key)\"" + exportPrim.ParticleSys.Texture.ToString() + "\"," + Environment.NewLine);
                            lsl.Append("         PSYS_SRC_TARGET_KEY, (key)\"" + exportPrim.ParticleSys.Target.ToString() + "\"" + Environment.NewLine);

                            lsl.Append("         ]);" + Environment.NewLine);
                            lsl.Append("    }" + Environment.NewLine);
                            lsl.Append("}" + Environment.NewLine);

                            #endregion Particle System to LSL

                            return lsl.ToString();
                        }
                        else
                        {
                            return "Prim " + exportPrim.LocalID + " does not have a particle system";
                        }
                    }
                }
            }

            return "Couldn't find prim " + id.ToString();
        }
    }
}
