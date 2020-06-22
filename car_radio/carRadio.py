# sources for object tracking
# https://www.pyimagesearch.com/2015/09/14/ball-tracking-with-opencv/
# https://piofthings.net/blog/opencv-baby-steps-4-building-a-hsv-calibrator

# import the necessary packages
from collections import deque
from imutils.video import VideoStream
import numpy as np
import cv2
import imutils
import time
import pickle
import serial
from serial import Serial;
import sys
import random
import glob


# --------------parameters------------------------

portName = 'COM4' #at the moment port number have to be given manually, needs further improvement

link = 0 #video source, can be webcam: 0 or 1, or video file: 'show.mp4' 
flip = False #flips the image by Y  axis if neccesary

rec = True#if true records webcam footage
recordName = 'continuos.avi' #filename for the footage

verbose = False #if true, prints main command
writeCommand = False #all commands to the arduina can be written out to the screen

line = True #tracing line
tracingLine = 64 #length of the tracing line

maxX = 640  # width
maxY = 480  # height

# switch for getting picture measurements only once
switch = True

#-------------------Start Radio Station ---------------------------
#
# source: http://zguide.zeromq.org/py:wuserver
#

import zmq
from random import randrange

context = zmq.Context()
socket = context.socket(zmq.PUB)
socket.bind("tcp://*:5555")
#---------------------------------------------------------------------

#-------------------------------------

def detectColor(colorToDetect):
    '''lets the user set the boundaries for a color using the HSV colorspace then save it into a pickle file
    @colorToDetect: the name of the .pckl file'''
    def callback(x):
        '''used as onChange event, does nothing'''
        pass

    cap = cv2.VideoCapture(link)
    cv2.namedWindow('image')

    #loading pckl file contents into variables
    f = open(str(colorToDetect) + '.pckl', 'rb')
    obj = pickle.load(f)
    f.close()
    ilowH = obj[0]
    ihighH = obj[1]
    ilowS = obj[2]
    ihighS = obj[3]
    ilowV = obj[4]
    ihighV = obj[5]

    # create trackbars for color change
    cv2.createTrackbar('lowH', 'image', ilowH, 179, callback)
    cv2.createTrackbar('highH', 'image', ihighH, 179, callback)

    cv2.createTrackbar('lowS', 'image', ilowS, 255, callback)
    cv2.createTrackbar('highS', 'image', ihighS, 255, callback)

    cv2.createTrackbar('lowV', 'image', ilowV, 255, callback)
    cv2.createTrackbar('highV', 'image', ihighV, 255, callback)

    while (True):
        # grab the frame
        ret, frame = cap.read()
        if flip:
            frame = cv2.flip(frame, 1)
        frame = imutils.resize(frame, width=640)

        # get trackbar positions
        ilowH = cv2.getTrackbarPos('lowH', 'image')
        ihighH = cv2.getTrackbarPos('highH', 'image')
        ilowS = cv2.getTrackbarPos('lowS', 'image')
        ihighS = cv2.getTrackbarPos('highS', 'image')
        ilowV = cv2.getTrackbarPos('lowV', 'image')
        ihighV = cv2.getTrackbarPos('highV', 'image')

        hsv = cv2.cvtColor(frame, cv2.COLOR_BGR2HSV)
        lower_hsv = np.array([ilowH, ilowS, ilowV])
        higher_hsv = np.array([ihighH, ihighS, ihighV])

        #show only the parts of the image within the boundaries
        mask = cv2.inRange(hsv, lower_hsv, higher_hsv)

        frame = cv2.bitwise_and(frame, frame, mask=mask)
        # show thresholded image
        cv2.imshow('detected', frame)
        k = cv2.waitKey(30) & 0xFF  # !!!!! set to 100 if color calibration freeze
        if k == 115: #s
            print('calibration result saved')
            #save results to .pckl
            obj = [ilowH, ihighH, ilowS, ihighS, ilowV, ihighV]
            f = open(str(colorToDetect) + '.pckl', 'wb')
            pickle.dump(obj, f)
            f.close()
            break
        if k == 113 or k == 27: #q or esc
            break

    cv2.destroyAllWindows()
    cap.release()

#---------------------------------------------------------------------------------
def trackObject(lower, upper, pts, line):
    '''gets the centerpoint of an object, alse creates tracking line
    @lower lowest values of HSV range
    @upper upper values of HSV range
    @pts list of points for the line
    @line boolean, true if tracing line is on'''
    mask = cv2.inRange(hsv, lower, upper)
    mask = cv2.erode(mask, None, iterations=2)
    mask = cv2.dilate(mask, None, iterations=2)

    # find contours in the mask and initialize the current
    # (x, y) center of the ball
    cnts = cv2.findContours(mask.copy(), cv2.RETR_EXTERNAL,
                            cv2.CHAIN_APPROX_SIMPLE)
    cnts = imutils.grab_contours(cnts)
    center = None

    # only proceed if at least one contour was found
    if len(cnts) > 0:
        # find the largest contour in the mask, then use
        # it to compute the minimum enclosing circle and
        # centroid
        c = max(cnts, key=cv2.contourArea)
        ((x, y), radius) = cv2.minEnclosingCircle(c)
        M = cv2.moments(c)
        center = (int(M["m10"] / M["m00"]), int(M["m01"] / M["m00"]))

        # only proceed if the radius meets a minimum size
        if radius > 1:
            # draw the circle and centroid on the frame,
            # then update the list of tracked points
            cv2.circle(frame, (int(x), int(y)), int(radius),
                       (0, 255, 255), 2)
            cv2.circle(frame, center, 5, (0, 0, 255), -1)

    # update the points queue
    pts.appendleft(center)

    if line:
        # loop over the set of tracked points
        for i in range(1, len(pts)):
            # if either of the tracked points are None, ignore
            # them
            if pts[i - 1] is None or pts[i] is None:
                continue

            # otherwise, compute the thickness of the line and
            # draw the connecting lines
            thickness = int(np.sqrt(tracingLine / float(i + 1)) * 2.5)
            cv2.line(frame, pts[i - 1], pts[i], (0, 0, 255), thickness)

    if center is None:
        return None, None
    else:
        return center[0], center[1]

#--------------------------------------------------------------------------------------------------
#----------------------- This is the end of function definitions ------------------------------------------------
#-----------------------------------and the beginning of-------------------------------------------------------------
#-----------------------------------------THE CODE----------------------------------------------------------------------

#printing instructions
f = open('instructions.txt', 'r')
file_contents = f.read()
print (file_contents)
f.close()

#print video source
print('The video source is: ' + str(link))

# -------------------- calibrating colors --------------------------
if input("Do you want to calibrate tracing colors? (y/n)\n")=="y":
    start = input("enter g + enter to set values for green, or press enter to continue,\nafter calibration press s to save results or q to quit \n \n")
    if start == "g":
        colorToDetect = 'green'
        detectColor(colorToDetect)
    start = input("\n enter b + enter to set values for blue, or press enter to continue,\nafter calibration press s to save results or q to quit \n \n")
    if start == "b":
        colorToDetect = 'blue'
        detectColor(colorToDetect)

# load color boundaries from pickle
f = open('green.pckl', 'rb')
obj = pickle.load(f)
f.close()
greenLower = (obj[0], obj[2], obj[4])
greenUpper = (obj[1], obj[3], obj[5])
f = open('blue.pckl', 'rb')
obj = pickle.load(f)
f.close()
blueLower = (obj[0], obj[2], obj[4])
blueUpper = (obj[1], obj[3], obj[5])

# lenght of storing previous positions (used for tracing line)
pts = deque(maxlen=tracingLine)
pts2 = deque(maxlen=tracingLine)
pts3 = deque(maxlen=tracingLine)

# set capture object
vs = cv2.VideoCapture(link)


# Video recording, might require additional installs
# Types of Codes: http://www.fourcc.org/codecs.php
if rec:
    fourcc = cv2.VideoWriter_fourcc(*'XVID')
    out = cv2.VideoWriter(recordName, fourcc, 24, (640, 480))
    #out = cv2.VideoWriter(recordName, cv2.VideoWriter_fourcc(*'XVID'), 24, (width, height))

# allow the camera or video file to warm up
time.sleep(2.0)

print("\nHold 'q' to quit\n")


# main loop
while True:
    # grabs frame, then converts it
    # grab the current frame
    frame = vs.read()
    # handle the frame from VideoCapture or VideoStream
    frame = frame[1]

    # if we are viewing a video and we did not grab a frame,
    # then we have reached the end of the video
    if frame is None:
        break

    #Flip the frame by the Y axis if neccessary
    if flip:
        frame = cv2.flip(frame, 1)

    #record
    if rec:
        out.write(frame)

    # resize the frame, blur it, and convert it to the HSV
    # color space
    frame = imutils.resize(frame, width=640)
    blurred = cv2.GaussianBlur(frame, (11, 11), 0)
    hsv = cv2.cvtColor(blurred, cv2.COLOR_BGR2HSV)

    # getting picture measurements
    if switch:
        height, width, channels = frame.shape
        maxX = width
        maxY = height

        print('The frame width is ' + str(int(width)) + 'px and frame height is: ' + str(int(height)) + 'px\n')
        switch = False

    # getting the car position (center of blue and center of green)
    carFrontX, carFrontY = trackObject(blueLower, blueUpper, pts, line)
    carBackX, carBackY = trackObject(greenLower, greenUpper, pts2, False)

    #Send Radio Signal ---------------------
    if carFrontX is None or carBackX is None:
        carFrontX = carFrontY = carBackX = carBackY = 0
    socket.send_string("%i %i %i %i %i" % (22, carFrontX, carFrontY, carBackX, carBackY))

    #----------------------maybe useful-------------------------------------#
    #if carFrontX is not None and carBackX is not None:
    

    # show the frame to our screen
    cv2.imshow("Frame", frame)
    key = cv2.waitKey(1) & 0xFF

    # if the 'q' key is pressed, stop the loop
    if cv2.waitKey(1) & 0xFF == ord('q'):
        print('q is pressed')
        break

# close all windows
stop()
try:
    out.release()
    camera.release()
except:
    pass
cv2.destroyAllWindows()


