using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class NoiseImage : MonoBehaviour
{
  public float strokeLength = 25;
  public float noiseScale = 0.005f;
  public float strokeMaxThickness = 15;

  //var imgNames = ["portrait.jpg", "fleurs.jpg", "frise.png" , "oiso.png", "arli.jpg" , "sirene.jpg"];
  public Texture2D image;
  Color[] imagePixels;

  public int drawLength = 250;  // The amount of times we'll iterate through draw() until it stops.
  int frame;  // Variable to contain the current frame.


  Vector3 margins;

  void Start() 
  {
    // INIT drawing system
    Draw.infiniteMode = true;
    Draw.EnableTransparentMode(true);
    Draw.SetValuesAsPixels(true);
    changeImage();


    // compute margins for the image to be centered
    Vector3 margins = new Vector3(
      (Screen.width - image.width) * 0.5f,
      (Screen.height - image.height) * 0.5f,
      0
    );
    // prevent the margins to be negative (can happen is the screen size is smaller than the size of the image)
    margins.x = Mathf.Max(margins.x, 0);
    margins.y = Mathf.Max(margins.y, 0);
  }


  void Update() {
    if(Input.GetKeyDown(KeyCode.R))
    {
      frame = 0;
      Draw.ClearDrawBuffer();
    }

    //Draw.Line2D(Vector3.zero, new Vector3(Screen.width, Screen.height));
    //Draw.Rect2D(Screen.width*0.5f, Screen.height * 0.5f, 300, 100);
    //Draw.Rect2D(Screen.width*0.5f, Screen.height * 0.5f, 300, 100, 30);
    //Draw.Circle2D(Screen.width*0.5f, Screen.height * 0.5f, 50);

      // Stop drawing once we exceed the drawing length.
    if (frame > drawLength) {
      return;
    }

    //let img = imgs[imgIndex];
     
    // The smaller the stroke is the more the spawn count increases to capture more detail.
    // map n'existe pas dans unity, il faut faire en deux étapes
    int count = (int)map(frame, 0, drawLength, 2, 80);

    // Add a loop to create multiple strokes.
    for (int i = 0; i < count; i++) 
    {  
      // get a random pixel inside the image
      int x = Random.Range(0, image.width);
      int y = Random.Range(0, image.height);
      int index = (y * image.width + x); // convert x-y coordinate into one-dimensionnal array index
      Color c = imagePixels[index];
      Draw.color = c;

      // Map the thickness based on the current frame of the sketch.
      // First it starts off thick, then gradually thins out until it reaches zero.
      int strokeThickness = (int)map(frame, 0, drawLength, strokeMaxThickness, 0);
      Draw.stroke = strokeThickness;

      float n = Mathf.PerlinNoise(x * noiseScale, y * noiseScale);
      float rotationAngle = map(n, 0, 1, -180, 180);
      float lengthVariation = Random.Range(0.75f, 1.25f);  // Randomize a multiplier to make length shorter or longer.

      // map the position inside the screen (screen may not be the same size as the image)
      Vector3 position = new Vector3(x, y,0);
      position.x = map(position.x, 0, image.width, margins.x, Screen.width - margins.x);
      position.y = map(position.y, 0, image.height, margins.y, Screen.height - margins.y);
      Draw.RoundedLine2D(position, rotationAngle, strokeLength * lengthVariation);
      //Draw.Circle(offset + position, strokeLength * lengthVariation * 0.2f, Vector3.back);
    }

    frame++;  // Increase frame by 1.
  }


  void changeImage() {
    frame = 0;
    imagePixels = image.GetPixels();
  }

  // map n'existe pas dans unity, on ajoute donc la fonction ici
  float map(float val, float sourcemin, float sourcemax, float destmin, float destmax)
  {
    float percent = Mathf.InverseLerp(sourcemin, sourcemax, val);
    return Mathf.Lerp(destmin, destmax, percent);
  }
}
