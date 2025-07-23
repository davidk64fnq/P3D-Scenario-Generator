// prepar3d-globals.d.ts

/**
 * Retrieves a variable's value from the Prepar3D simulator environment.
 * @param varName - The name of the Prepar3D variable to retrieve.
 * @param varType - The expected type of the variable (e.g., "NUMBER", "STRING", "Radians").
 * @returns The value of the Prepar3D variable.
 */
declare function VarGet(varName: string, varType: string): any;

// You can add other global functions or variables provided by P3D here if needed in the future
// declare var SimConnect: { ... }; // Example for a global SimConnect object