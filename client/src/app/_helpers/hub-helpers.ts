export class HubHelpers {
  static waitForHubConnection(hubConnection: any, maxWaitTime: number = 20000): Promise<void> {
    const startTime = Date.now();

    return new Promise<void>((resolve, reject) => {
      const checkConnection = () => {
        if (hubConnection && hubConnection.state === 'Connected') {
          resolve();
        } else if (Date.now() - startTime >= maxWaitTime) {
          reject(new Error('Timeout waiting for hub connection'));
        } else {
          setTimeout(checkConnection, 100);
        }
      };

      checkConnection();
    });
  }
}