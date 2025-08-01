using P3D_Scenario_Generator.CelestialScenario;

namespace P3D_Scenario_Generator
{
    /// <summary>
    /// Handles the generation and management of simulator-specific files,
    /// primarily the "stars.dat" file, to ensure consistency between
    /// the celestial sextant display and the in-simulator view of stars.
    /// It provides functionality to backup the original "stars.dat" and
    /// replace it with a program-generated version containing relevant star data.
    /// </summary>
    internal class SimulatorFileGenerator
    {
        /// <summary>
        /// Creates P3D Scenario Generator specific version of "stars.dat" and backs up original if user agrees. If user doesn't agree
        /// a dummy copy of "stars.dat" is created so that user isn't asked again.
        /// </summary>
        /// <returns>True if all needed file operations complete successfully</returns>
        static internal bool CreateStarsDat(ScenarioFormData formData)
        {
            string starsDatPath = Path.Combine(formData.P3DProgramData, "stars.dat");
            string starsDatBackupPath = Path.Combine(formData.P3DProgramData, "stars.dat.P3DscenarioGenerator.backup");

            string starsDatContent = $"[Star Settings]\nIntensity=230\nNumStars={StarDataManager.noStars}\n[Star Locations]\n";

            // If the file "stars.dat.P3DscenarioGenerator.backup" exists then assume user has previously said yes to the prompt below
            // and there is no need to do it again as the replacement "stars.dat" doesn't change over time
            if (!File.Exists(starsDatBackupPath))
            {
                string message = "To see the same stars out the window as are displayed in the celestial sextant, the program" +
                                    " needs to backup the existing stars.dat file (to stars.dat.P3DscenarioGenerator.backup) and replace" +
                                    " it with a program generated version. Press \"Yes\" to go ahead with backup and replacement, \"No\" to leave stars.dat as is";
                string title = "Confirm backup and replacement of stars.dat";
                MessageBoxButtons buttons = MessageBoxButtons.YesNo;
                DialogResult result = MessageBox.Show(message, title, buttons, MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    // Backup existing stars.dat if it exists
                    if (File.Exists(starsDatPath))
                    {
                        // Try to delete the old backup. If it fails, report and stop.
                        if (!FileOps.TryDeleteFile(starsDatBackupPath, null))
                        {
                            return false;
                        }

                        // Try to move the current stars.dat. If it fails, report and stop.
                        if (!FileOps.TryMoveFile(starsDatPath, starsDatBackupPath, null))
                        {
                            return false;
                        }
                    }

                    // Populate the content for the new stars.dat
                    for (int index = 0; index < StarDataManager.noStars; index++)
                    {
                        starsDatContent += $"Star.{index} = {index + 1}";
                        starsDatContent += $",{StarDataManager.stars[index].RaH}";
                        starsDatContent += $",{StarDataManager.stars[index].RaM}";
                        starsDatContent += $",{StarDataManager.stars[index].RaS}";
                        starsDatContent += $",{StarDataManager.stars[index].DecD}";
                        starsDatContent += $",{StarDataManager.stars[index].DecM}";
                        starsDatContent += $",{StarDataManager.stars[index].DecS}";
                        starsDatContent += $",{StarDataManager.stars[index].VisMag}\n";
                    }

                    // Try to write the new stars.dat file. If it fails, report and stop.
                    if (!FileOps.TryWriteAllText(starsDatPath, starsDatContent, null))
                    {
                        return false;
                    }

                    MessageBox.Show("stars.dat successfully updated and backed up.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return true; // Operation successful
                }
                else // Copy "stars.dat" to "stars.dat.P3DscenarioGenerator.backup" to prevent future prompting of user
                {
                    if (!FileOps.TryCopyFile(starsDatPath, starsDatBackupPath, null, false))
                    {
                        return false;
                    }
                }
            }
            return true;
        }
    }
}
