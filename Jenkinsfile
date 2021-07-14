node {
    stage('Clone repository') {
        git branch: 'main', credentialsId: 'github-app-SilKsPlugins', url: 'https://github.com/SilKsPlugins/SilKsPlugins.DiscordBot'
    }
    
    stage('Login to GitHub Container Registry') {
        withCredentials([usernamePassword(credentialsId: 'GitHub-SilKsPlugins-Packages', passwordVariable: 'PAT', usernameVariable: 'USERNAME')]) {
            sh '''
                echo $PAT | docker login ghcr.io -u $USERNAME --password-stdin
            '''
        }
    }

    stage('Pull container') {
        sh '''
            docker pull ghcr.io/silksplugins/silksplugins-discordbot:latest
        '''
    }

    stage('Deploy container') {
        sh '''
            docker-compose up -d
        '''
    }
}