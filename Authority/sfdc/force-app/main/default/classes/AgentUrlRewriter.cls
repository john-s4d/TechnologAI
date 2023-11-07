global without sharing class AgentUrlRewriter implements Site.UrlRewriter {

    global AgentUrlRewriter () { }
    
    global List<PageReference> generateUrlFor(List<PageReference> salesforceUrls) {

        //System.Debug(salesforceUrls);
        
        return salesforceUrls;
    }
    
    global PageReference mapRequestUrl(PageReference friendlyUrl) {
    
        //System.Debug(friendlyUrl);        

        return friendlyUrl;
    }
}