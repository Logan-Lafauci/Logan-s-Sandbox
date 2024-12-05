# Logan's Sandbox Overview
This is a personal project where I'm learning how to create video game mechanics I find interesting. 
This project is being made in the Unity game engine with the hope to create generic scripts that can be used in future projects. 
This all started thanks to weekly engineering meetings held at **Pixel Dash Studios** and started by **Curtis Cummings**.

# Mechanics
This section is where I will talk in detail about the inspiration to make certain mechanics and my progress made on them.

## Portals 
[Reference Video](https://www.youtube.com/watch?v=cWpFZbjtSQg&list=PLtHUgNJAbnQWarSv_sn362oljeHvpVRT7&index=28)

**Found In Assets/Scripts/Portals**

Portals were the biggest inspiration to help start this project. I played portal when I was a kid on the playstation 3 and it was the first mechanic to blow my mind.
I had a feeling most of the work for this mechanic would come down to visuals which gave me a good excuse to work with shaders. 
I found a video by **Sebastian Lague** that shows his implementation and I used that to learn how to create the mechanic. 
This video provided many formulas for converting relative variables to another object to aid in the seamless transition.

The first major hurdle I had to overcome was figuring out model UV's and how to edit them on Blender.
After fixing issues with that I had to learn how to read shader code to implement some minor changes so I can remove lighting effects like shadows, be able to texture a material with a camera, 
and change the bounds of the texture so the camera shows the view beyond the portal's gate.
Once I had a camera displaying the other side of the portal I applied a function that made the portal's camera mimic the players perspective so traveling through the portal would appear seamless.

After getting the basic look of the portal down it was time to add functionality.
This was accomplished by using a dot product to detect when an object passed through the portal. Once the object passed through the transform of the object was set to the linked portal. 
One issue I ran into with this was the rigidbody pushing the player back into the portal. 
I fixed this by using a formula that **Sebastian Lague** used for his rigidbody objects toPortal.TransformVector(fromPortal.InverseTransformVector(rb.linearVelocity)).
That function takes the velocity going into a portal and sets it relative to the portal the object is coming out of.
Another issue I had to fix was rotating the player's camera to line up with the camera shown on the portal's screen.
I fixed this by applying my own modified version of a formula that changes the portal camera's position. ***(toPortal.transform.localToWorldMatrix * fromPortal.transform.worldToLocalMatrix * playerCamera.transform.localToWorldMatrix).rotation;***

Once teleportation was working properly I moved onto polishing up some visuals by heavily following the video and using his **Sebastian Lague** [github repository](https://github.com/SebLague/Portals/tree/master) as a base. 
I then created a modified version of the script that renders from the portal the player is looking at instead of the linked portal.
I did this so I would be able to create a chain of portals instead of 2 portals that are linked to each other. 
This allows for fun level design like an infinite hallway that the player escapes by going backwards or setting up identical rooms with slight differences that are all connected.
