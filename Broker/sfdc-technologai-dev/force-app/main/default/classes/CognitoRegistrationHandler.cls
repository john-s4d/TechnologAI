public class CognitoRegistrationHandler implements Auth.RegistrationHandler{

    private string USERNAME_SUFFIX = '.technologai-dev';
    private string NEW_USER_PROFILE_NAME = 'Technologai Community User';

    public User createUser(Id portalId, Auth.UserData data) {
    
        System.debug(data);    
        System.debug(data.attributeMap);
    
        if (!data.attributeMap.containsKey('email') || string.isBlank(data.attributeMap.get('email'))) {
            throw new Auth.AuthProviderPluginException('Email was not provided');
        }
   
        if (data.attributeMap.containsKey('email')) {
        
            User[] existingUsers = [
                SELECT Id FROM User
                WHERE Email = :data.attributeMap.get('email')
                LIMIT 1
            ];

            if (existingUsers.size() > 0) {                
                throw new Auth.AuthProviderPluginException('User already exists in Salesforce with email: ' + data.attributeMap.get('email'));
            }
        }

        User newUser = new User();
        newUser.FederationIdentifier = data.attributeMap.get('sub');
        newUser.Username = data.attributeMap.get('email') + USERNAME_SUFFIX;
        newUser.Email = data.attributeMap.get('email');
        newUser.LastName = data.attributeMap.get('family_name');
        newUser.FirstName = data.attributeMap.get('given_name');
        newUser.Alias = createAliasFromEmail(data.attributeMap.get('email'));
        newUser.CommunityNickname = newUser.Alias;
        newUser.TimeZoneSidKey = 'America/Los_Angeles';
        newUser.LocaleSidKey = 'en_US';
        newUser.EmailEncodingKey = 'UTF-8';
        newUser.ProfileId = getProfileId(NEW_USER_PROFILE_NAME);
        newUser.LanguageLocaleKey = 'en_US';

        insert newUser;
        return newUser;
    }

    public void updateUser(Id userId, Id portalId, Auth.UserData data) {
    
        System.debug(data);    
        System.debug(data.attributeMap);
    
        User userToUpdate = [SELECT Id, FederationIdentifier, Username, Email, LastName, FirstName, Alias, CommunityNickname FROM User WHERE Id = :userId LIMIT 1][0];
        
        userToUpdate.FederationIdentifier = data.attributeMap.get('sub');        
        userToUpdate.Username = data.attributeMap.get('email') + USERNAME_SUFFIX;
        userToUpdate.Email = data.attributeMap.get('email');
        userToUpdate.LastName = data.attributeMap.get('family_name');
        userToUpdate.FirstName = data.attributeMap.get('given_name');
        userToUpdate.Alias = createAliasFromEmail(data.attributeMap.get('email'));
        userToUpdate.CommunityNickname = userToUpdate.Alias;
        
        update userToUpdate;
    }

    private String createAliasFromEmail(String email) {
        String alias = email.substring(0, email.indexOf('@'));
        alias = alias.replace('.', '');
        alias = alias.substring(0, Math.min(alias.length(), 8));
        return alias;
    }

    private Id getProfileId(String profileName) {
        Profile[] profiles = [SELECT Id FROM Profile WHERE Name = :profileName];
        return (profiles.size() > 0) ? profiles[0].Id : null;
    }
}