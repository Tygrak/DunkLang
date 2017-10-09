#include <GL/glew.h> // Include the GLEW header file  
#include <GL/glut.h> // Include the GLUT header file  
  
int keyStates[256]; // Create an array of boolean values of length 256 (0-255)  

void Initialize(){
    /*glMatrixMode(GL_PROJECTION);
    glLoadIdentity();
    glOrtho(0.0, 100.0, 0.0, 100.0, 0.0, 100.0);*/
}

void KeyOperations(void){  
    if (keyStates[27]){ // If the 'a' key has been pressed  
        exit(0);
    }  
}  
  
void Display(void){  
    KeyOperations();  
    glClearColor(0.0f, 0.0f, 0.1f, 1.0f); // Clear the background of our window to red  
    glClear(GL_COLOR_BUFFER_BIT); //Clear the colour buffer (more buffers later on)  
    glLoadIdentity();
    glColor3f(1.0, 1.0, 1.0);
    /*glBegin(GL_POLYGON);
    glVertex3f(0.1f, 0.1f, 1.0);
    glVertex3f(0.9f, 0.1f, 1.0);
    glVertex3f(0.5f, 0.9f, 1.0);
    glEnd();*/
    glFrontFace(GL_CW);
    glutWireTeapot(0.5f);
    glFrontFace(GL_CCW);
    glFlush(); // Flush the OpenGL buffers to the window  
}  
  
void Reshape(int width, int height){  
    glViewport(0, 0, (GLsizei)width, (GLsizei)height); // Set our viewport to the size of our window  
    /*glMatrixMode(GL_PROJECTION);
    glLoadIdentity();
    glOrtho(0.0, 1.0, 0.0, 1.0, 0.0, 1.0);*/
}  
  
void KeyPressed(unsigned char key, int x, int y){  
    keyStates[key] = 1; // Set the state of the current key to pressed  
}  
  
void KeyUp(unsigned char key, int x, int y){  
    keyStates[key] = 0; // Set the state of the current key to not pressed  
}  
  
int main(int argc, char **argv){  
    glutInit(&argc, argv); // Initialize GLUT  
    glutInitDisplayMode (GLUT_SINGLE); // Set up a basic Display buffer (only single buffered for now)  
    glutInitWindowSize (500, 500); // Set the width and height of the window
    glutCreateWindow ("DunkWindow"); // Set the title for the window
    glutDisplayFunc(Display); // Tell GLUT to use the method "Display" for rendering  
    glutReshapeFunc(Reshape); // Tell GLUT to use the method "Reshape" for reshaping  
    glutKeyboardFunc(KeyPressed); // Tell GLUT to use the method "KeyPressed" for key presses  
    glutKeyboardUpFunc(KeyUp); // Tell GLUT to use the method "KeyUp" for key up events  
    glutMainLoop(); // Enter GLUT's main loop  
}  