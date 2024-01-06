# VehiclePhyModeler
VehiclePhyModeler is the first tool in the CarCustomizer tool set, and allows modification of the collision and wheels of a Trackmania car.
# How to use
1. Drag a `VehiclePhyModel.Gbx` file onto the executable, the hitbox will be extracted to a human-readable JSON file.
2. Edit the JSON, or simply keep everything as it is.
3. Drag the JSON back onto the executable, together with another `VehiclePhyModel.Gbx` which will work as a base for the output file. Feel free to mix environments like in this example, where I combine the Lagoon hitbox with Stadium handling! ( => JSON: hitbox, Gbx: handling)

<p align="center"><img src="https://drive.google.com/uc?export=download&id=17NwNgNlHRHa2LT23Omm3YRV4AYTREo3_"/></p>

# Disclaimer
The tool is EXTREMELY limited at the moment.
1. It will only work with `VehiclePhyModel.Gbx` files which include the `Shape.Gbx` file, some files of the TM United cars come with this file externally, which is not yet supported.
2. The `Type` value of the collision elements can only be set to `Ellipsoid`, which is a sphere with a radius of `1.0` that gets scaled on each axis by the values given in `Parameters`. This should be sufficient for nearly every car that exists or which you would like to create, but might change in the future.
3. The `Rotation` value measures mathematical rotation with the help of quaternions. I recommend Online converters to get the correct values from Euler angles or similar.
4. Simply extracting and reimporting the hitbox of an original TM vehicle will likely break "Press Forward" maps due to floating point precision errors, however driving the car normally should basically feel no different to before. This will probably not change until after a major revamp of the tool set.

# Building
Oh boy. It won't be a simple drag 'n' drop, as I had to modify the GBX.NET library as well. You will find the file with the source.
For building I use .net 8.0
