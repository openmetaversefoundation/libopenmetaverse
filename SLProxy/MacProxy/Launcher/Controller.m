#import "Controller.h"

@implementation Controller

+ (void)terminateOnFailure:(NSTask *)task {
	killtask = task;
}

+ (void)failBecause:(NSString *)reason {
	NSRunCriticalAlertPanel(@"Failed to start Second Life.", reason, @"Quit", nil, nil);
	[NSApp terminate:nil];
}

- (void)runSecondLifeWithLoginURL:(NSURL *)URL {
	/* Locate the user's installed copy of Second Life. */
	CFURLRef SLAppURL;
	if (LSFindApplicationForInfo(kLSUnknownCreator, NULL, (CFStringRef)@"Second Life.app", NULL, &SLAppURL)) {
		[Controller failBecause:@"Second Life does not appear to be installed on your system."];
	}
	
	/* Launch Second Life and wait until it terminates. */
	NSTask *task = [[NSTask alloc] init];
	[task setLaunchPath:[[(NSURL *)SLAppURL path] stringByAppendingString:@"/Contents/MacOS/Second Life"]];
	[task setArguments:[NSArray arrayWithObjects:@"-loginuri", [URL absoluteString], nil]];
	[task launch];
	[task waitUntilExit];
	[task release];
}

- (id)init {
	[super init];
	killtask = nil;
	return self;
}

- (void)awakeFromNib {
	[NSApp setDelegate:self];
}

- (void)applicationDidFinishLaunching:(NSNotification *)notification {
	/* Start the proxy, run Second Life, stop the proxy, and terminate. */
	SLProxy *proxy = [[SLProxy alloc] init];
	[self runSecondLifeWithLoginURL:[proxy loginURL]];
	[proxy release];
	[NSApp terminate:self];
}

- (void)applicationWillTerminate:(NSNotification *)notification {
	[killtask terminate];
}

@end
