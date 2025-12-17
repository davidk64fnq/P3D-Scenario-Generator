// prepar3d-globals.d.ts

/**
 * Retrieves a variable's value from the Prepar3D simulator environment.
 * @param varName - The name of the Prepar3D variable to retrieve.
 * @param varType - The expected type of the variable (e.g., "NUMBER", "STRING", "Radians").
 * @returns The value of the Prepar3D variable.
 */
declare function VarGet(varName: string, varType: string): any;

/**
 * @summary Sets a variable in the flight simulator.
 * @param {string} varName - The name of the simulator variable to set (e.g., "S:errorMsgVar").
 * @param {string} units - The units in which the value is provided (e.g., "NUMBER").
 * @param {number | string} value - The value to set the simulator variable to.
 */
declare function VarSet(varName: string, units: string, value: number | string): void;