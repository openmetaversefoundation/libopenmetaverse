#import "Controller.h"

@implementation Controller

+ (void)failBecause:(NSString *)reason {
	NSRunCriticalAlertPanel(@"Failed to start Second Life.", reason, @"Quit", nil, nil);
	[NSApp terminate:nil];
}

- (void)runSecondLifeWithLoginURL:(NSURL *)URL {
	CFURLRef SLAppURL;
	if (LSFindApplicationForInfo(kLSUnknownCreator, NULL, (CFStringRef)@"Second Life.app", NULL, &SLAppURL)) {
		[Controller failBecause:@"Second Life does not appear to be installed on your system."];
	}
	
	NSTask *task = [[NSTask alloc] init];
	[task setLaunchPath:[[(NSURL *)SLAppURL path] stringByAppendingString:@"/Contents/MacOS/Second Life"]];
	[task setArguments:[NSArray arrayWithObjects:@"-loginuri", [URL absoluteString], nil]];
	[task launch];
	[task waitUntilExit];
	[task release];
}

- (void)awakeFromNib {
	SLProxy *proxy = [[SLProxy alloc] init];
	[self runSecondLifeWithLoginURL:[proxy loginURL]];
	[proxy release];
	[NSApp terminate:self];
}

@end
