#!/usr/bin/env python
# -*- coding: utf-8 -*-
# robot_joints = [head, neck, torso, left_shoulder, left_elbow, left_hand, right_shoulder, right_elbow,　right_hand, left_hip, left_knee, left_foot, right_hip, right_knee, right_foot]

import threading
import time
import rospy
import numpy as np
from geometry_msgs.msg import Twist
from skeleton_markers.msg import Skeleton
from socket import SOCK_STREAM, socket, AF_INET

SERVER_IP   = '192.168.3.15'
PORT_NUMBER = 7000
SIZE = 1024

def cosine(vec1, vec2): #calculates the cosine of the angle between two vectors
    return float(np.dot(vec1, vec2)/(np.linalg.norm(vec1)*np.linalg.norm(vec2)))

#initiate the node which subscribes to kinect and publishes velocity
rospy.init_node('subscriber_publisher')
pub = rospy.Publisher("/mobile_base/commands/velocity", Twist)
left_history = []
right_history = []


def callback(msg): #callback function
    #coordinates of joints
    right_shoulder = np.array([msg.position[6].x, msg.position[6].y])
    right_elbow = np.array([msg.position[7].x, msg.position[7].y])
    left_shoulder = np.array([msg.position[3].x, msg.position[3].y])
    left_elbow = np.array([msg.position[4].x, msg.position[4].y])
    right_hip = np.array([msg.position[12].x, msg.position[12].y])
    left_hip = np.array([msg.position[9].x, msg.position[9].y])

    #each angle in cosine
    """
    right_shoulder_cosine = cosine(right_elbow-right_shoulder, right_hip-right_shoulder)
    left_shoulder_cosine = cosine(left_elbow-left_shoulder, left_hip-left_shoulder)
    """
    left_shoulder_cosine = cosine(right_elbow-right_shoulder, right_hip-right_shoulder)
    right_shoulder_cosine = cosine(left_elbow-left_shoulder, left_hip-left_shoulder)

    global left_history
    global right_history
    while len(left_history) < 10:
        left_history.append(cosine(right_elbow-right_shoulder, right_hip-right_shoulder))
    left_average = sum(left_history) / float(len(left_history))
    left_history = []
    while len(right_history) < 10:
        right_history.append(cosine(left_elbow-left_shoulder, left_hip-left_shoulder))
    right_average = sum(right_history) / float(len(right_history))
    right_history = []
    

    if right_average < -0.7 and left_average < -0.7:
    #forward
        value = Twist()
        value.linear.x = 0.1
        pub.publish(value)
        s = socket(AF_INET, SOCK_STREAM)
        s.connect((SERVER_IP, PORT_NUMBER))
        s.send(b'run<EOF>')
        s.close()
    elif right_average < 0.4 and right_average >-0.4 and left_average > 0.7:
	#right rotation
        value = Twist()
        value.linear.x = -0.1
        pub.publish(value)
        s = socket(AF_INET, SOCK_STREAM)
        s.connect((SERVER_IP, PORT_NUMBER))
        s.send(b'reverse<EOF>')
        s.close()
    elif right_average > 0.7 and left_average < 0.4 and left_average > -0.4:
	#left rotation
        value = Twist()
        value.angular.z = np.sin(time.time())/10
        pub.publish(value)
        s = socket(AF_INET, SOCK_STREAM)
        s.connect((SERVER_IP, PORT_NUMBER))
        s.send(b'shake<EOF>')
        s.close()
    elif right_average < 0.4 and right_average >-0.4 and left_average < 0.4 and left_average > -0.4:
        value = Twist()
        value.angular.z = 0.0
        pub.publish(value)
        s = socket(AF_INET, SOCK_STREAM)
        s.connect((SERVER_IP, PORT_NUMBER))
        s.send(b'sit<EOF>')
        s.close()
    else:
        s = socket(AF_INET, SOCK_STREAM)
        s.connect((SERVER_IP, PORT_NUMBER))
        s.send(b'stand<EOF>')
        s.close()

    

sub = rospy.Subscriber('skeleton', Skeleton, callback)

rate = rospy.Rate(2)

rospy.spin()
