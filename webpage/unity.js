function UnityWebMediator() {
    this.android = navigator.userAgent.match(/Android/);
    this.ios = navigator.userAgent.match(/iPhone/) || navigator.userAgent.match(/iPod/) || navigator.userAgent.match(/iPad/);
    this.mac = navigator.userAgent.match(/Macintosh/);
    
    console.log(navigator.userAgent);

    this.messageQueue = Array();

    this.callback = function(path, args) {
        var message = path;
        
        if (args) {
            var stack = [];
            for (var key in args) {
                stack.push(key + "=" + encodeURIComponent(args[key]));
            }
            message += "?" + stack.join("&");
        }
        
        if (this.android) {
            UnityInterface.pushMessage(message);
        } else if (this.ios) {
	    console.log("iOS");
            this.messageQueue.push(message);
        } else if (this.mac) {
            console.log("Mac");
            this.messageQueue.push(message);
        } else {
            console.log(message);
        }
    };

    this.pollMessage = function() {
	return this.messageQueue.shift();
        
    }
    
    unityWebMediatorInstance = this;
}