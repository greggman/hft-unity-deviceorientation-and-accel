/*
 * Copyright 2014, Gregg Tavares.
 * All rights reserved.
 *
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions are
 * met:
 *
 *     * Redistributions of source code must retain the above copyright
 * notice, this list of conditions and the following disclaimer.
 *     * Redistributions in binary form must reproduce the above
 * copyright notice, this list of conditions and the following disclaimer
 * in the documentation and/or other materials provided with the
 * distribution.
 *     * Neither the name of Gregg Tavares. nor the names of its
 * contributors may be used to endorse or promote products derived from
 * this software without specific prior written permission.
 *
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS
 * "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT
 * LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR
 * A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT
 * OWNER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
 * SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT
 * LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE,
 * DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY
 * THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
 * OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 */
"use strict";

// Start the main app logic.
requirejs([
    'hft/commonui',
    'hft/gameclient',
    'hft/misc/input',
    'hft/misc/misc',
    'hft/misc/mobilehacks',
    'hft/misc/touch',
  ], function(
    CommonUI,
    GameClient,
    Input,
    Misc,
    MobileHacks,
    Touch) {

  var globals = {
    debug: false,
    orientation: "portrait-primary",
  };
  Misc.applyUrlSettings(globals);
  MobileHacks.fixHeightHack();
  MobileHacks.disableContextMenu();

  var score = 0;
  var inputElem = document.getElementById("inputarea");
  var colorElem = document.getElementById("display");
  var noElem = document.getElementById("nomotion");
  var client = new GameClient();

  CommonUI.setupStandardControllerUI(client, globals);

  var randInt = function(v) {
    return Math.floor(Math.random() * v);
  };

  // Pick a random color
  var color =  'rgb(' + randInt(256) + "," + randInt(256) + "," + randInt(256) + ")";
  // Send the color to the game.
  //
  // This will generate a 'color' event in the corresponding
  // NetPlayer object in the game.
  client.sendCmd('color', {
    color: color,
  });
  colorElem.style.backgroundColor = color;

  var node = new THREE.Object3D();
  var orientation = new THREE.Euler();
  var controls = new THREE.DeviceOrientationControls(node, true);
  controls.connect();
  controls.update();

  var quantize = function(v) {
    return Math.floor(v);
  };

  var sendDeviceAcceleration = function(eventData) {
    var accel    = eventData.acceleration || eventData.accelerationIncludingGravity;
    var rot      = eventData.rotationRate || { alpha: 0, gamma: 0, beta: 0};
    var interval = eventData.interval || 1;
    var msg = {
      x: quantize(accel.x   / interval),
      y: quantize(accel.y   / interval),
      z: quantize(accel.z   / interval),
      a: quantize(rot.alpha / interval),
      b: quantize(rot.beta  / interval),
      g: quantize(rot.gamma / interval),
    };

    client.sendCmd('accel', msg);
  };

  if (!window.DeviceMotionEvent) {
    noElem.style.display = "block";
    return;
  }

  window.addEventListener('devicemotion', sendDeviceAcceleration, false);

  var sendDeviceOrientation = function(eventData) {
    controls.setOrientation(eventData);
    controls.update();
    orientation.setFromQuaternion(node.quaternion, 'YXZ');
    client.sendCmd('rot', {
      x: orientation.x * 180 / Math.PI,
      y: orientation.y * 180 / Math.PI,
      z: orientation.z * 180 / Math.PI,
    });
  };

  if (!window.DeviceOrientationEvent) {
    noElem.style.display = "block";
    return;
  }

  window.addEventListener('deviceorientation', sendDeviceOrientation, false);
});

