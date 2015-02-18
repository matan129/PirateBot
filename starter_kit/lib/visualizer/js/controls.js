'use strict';

angular.module('visualizerApp', []);

angular.module('visualizerApp').controller('VisualizerCtrl', ['$scope', function($scope) {
    var fog = undefined;
    function init() {
        var options = new Options();
        options.data_dir = window.pirates_data_dir || '../visualizer/data/';
        options.embedded = true;
        options.local_run = window.local_run;
        options.playercolors = window.playercolors;
        if (!options.local_run) {
            options.turn = 0;
        }
        options.updateExternalView = function() {
            setTimeout(function() {
                $scope.$apply(function() {
                    $scope.turn = $scope.visualizer.state.time | 0;
                    $scope.playing = $scope.visualizer.director.playing();
                });
            }, 0);
        };
        setTimeout(function() {
            $scope.visualizer = new Visualizer(document.getElementById('game-canvas'), options);
            $scope.visualizer.loadReplayData(window.replayData);
        }, 0);
    }

    $scope.showFog = function(id) {
        if (fog !== id) {
            fog = id;
        } else {
            fog = undefined;
        }
        $scope.visualizer.showFog(fog);
    };

    $scope.play = function() {
        $scope.visualizer.director.play();
    };

    $scope.firstTurn = function() {
        $scope.visualizer.director.gotoTick(0);
    };

    $scope.lastTurn = function() {
        $scope.visualizer.director.gotoTick($scope.visualizer.director.duration);
    };

    $scope.prevTurn = function() {
        var stop = Math.ceil($scope.visualizer.state.time * 2) - 1;
        $scope.visualizer.director.slowmoTo(stop / 2);
    };

    $scope.nextTurn = function() {
        var stop = Math.floor($scope.visualizer.state.time * 2) + 1;
        $scope.visualizer.director.slowmoTo(stop / 2);
    };

    $scope.togglePlay = function() {
        $scope.visualizer.director.playStop();
        $scope.visualizer.draw();
    };

    $scope.speed = function(amount) {
        $scope.visualizer.modifySpeed(amount);
    };

    $scope.togglePiratesText = function() {
        var lbl = $scope.visualizer.state.config['label'];
        $scope.visualizer.setPirateLabels((lbl + 1) % 3);
        $scope.visualizer.director.draw();
    };

    init();
}]);