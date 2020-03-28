from django.contrib.auth.models import User
from rest_framework import serializers
from .models import GameUserMap
 
class UserSerializer(serializers.ModelSerializer):
 
    class Meta:
        model = User
        fields = ('id', 'username', 'first_name', 'last_name')
 
class GameUserMapSerializer(serializers.ModelSerializer):
    user = serializers.StringRelatedField(many=False)
 
    class Meta:
        model = GameUserMap
        fields = ('id', 'user', 'created_on', 'game')