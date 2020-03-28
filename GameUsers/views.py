from django.shortcuts import render
from django.contrib.auth.models import User
from rest_framework import viewsets, permissions
from .models import GameUserMap
from . import serializers
#from .permissions import ReadOnly
from rest_framework import permissions
 
def index(request, path=''):
    return render(request, 'index.html')
 
class UserViewSet(viewsets.ModelViewSet):
    """
    Provides basic CRUD functions for the User model
    """
    queryset = User.objects.all()
    serializer_class = serializers.UserSerializer
    #permission_classes = (ReadOnly,)
    permission_classes = (permissions.IsAuthenticatedOrReadOnly,)
 
class GameUserMapViewSet(viewsets.ModelViewSet):
    """
    Provides basic CRUD functions for the Blog Post model
    """
    queryset = GameUserMap.objects.all()
    serializer_class = serializers.GameUserMapSerializer
    permission_classes = (permissions.IsAuthenticatedOrReadOnly, )
 
    def perform_create(self, serializer):
        serializer.save(user=self.request.user)