#import "SLProxy.h"

#include <unistd.h>

@implementation SLProxy

- (SLProxy *)init {
	[super init];
	
	NSPipe *pipe = [NSPipe pipe];
	
	/* Launch the proxy. */
	task = [[NSTask alloc] init];
	[task setLaunchPath:@"/Library/Frameworks/Mono.framework/Commands/mono"];
	[task setCurrentDirectoryPath:[[[NSBundle mainBundle] resourcePath] stringByAppendingString:@"/Assemblies"]];
	[task setArguments:[[[NSBundle mainBundle] infoDictionary] valueForKey:@"MonoArguments"]];
	[task setStandardOutput:[pipe fileHandleForWriting]];
	NS_DURING
		[task launch];
	NS_HANDLER
		[Controller failBecause:@"Mono does not appear to be installed on your system."];
	NS_ENDHANDLER
	[Controller terminateOnFailure:task];
	
	/* Read the proxy's output to determine the login URL to give SL. */
	int reader = [[pipe fileHandleForReading] fileDescriptor];
	char c;
	NSString *line = [NSString string];
	for (;;) {
		if (read(reader, &c, 1) != 1) {
			[Controller failBecause:@"Unable to synchronize with proxy."];
		} else if (c == '\n') {
			break;
		} else {
			line = [line stringByAppendingFormat:@"%c", c];
		}
	}
	[[pipe fileHandleForReading] closeFile]; // mono's ok with this; keep it from blocking on WriteLine
	int port;
	if (sscanf([line UTF8String], "proxy ready at http://127.0.0.1:%d/", &port) != 1) {
		[Controller failBecause:@"Unable to synchronize with proxy."];
	}
	loginURL = [[NSURL alloc] initWithString:[NSString stringWithFormat:@"http://127.0.0.1:%d/", port]];
	
	return self;
}

- (void)dealloc {
	/* Stop the proxy and clean up. */
	[task terminate];
	[task release];
	[loginURL release];
	[super dealloc];
}

- (NSURL *)loginURL {
	return loginURL;
}

@end
