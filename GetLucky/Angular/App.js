﻿'use strict'
let myApp = angular.module('myApp', ['ui.router', 'wu.masonry', 'infinite-scroll', 'ngFileUpload', 'LocalStorageModule', 'angular-jwt']);

myApp.config(($stateProvider, $urlRouterProvider) => {
    $urlRouterProvider.otherwise('/home');
    $stateProvider
        .state('home', {
            url: '/home',
            templateUrl: 'views/home.html'
        })
        .state('blog', {
            url: '/blog',
            templateUrl: 'views/blog.html'
        })
        .state('signup', {
            url: '/signup',
            templateUrl: 'views/signup.html'
        })
        .state('login', {
            url: '/login',
            templateUrl: 'views/login.html'
        })
        .state('adminPanel', {
            url: '/adminPanel',
            templateUrl: 'views/adminPanel.html'
        })
        .state('otherwise', {
            url: '/home',
            templateUrl: 'views/home.html'
        });
});

myApp.run(function ($rootScope, $templateCache) {
    $rootScope.$on('$viewContentLoaded', function () {
        $templateCache.removeAll();
    });
});

myApp.config(function ($httpProvider) {
    $httpProvider.interceptors.push('authInterceptorService');
});